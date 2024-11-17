#region

using HttpExceptions.Exceptions;
using Microsoft.EntityFrameworkCore;
using rag_2_backend.Infrastructure.Common.Model;
using rag_2_backend.Infrastructure.Dao;
using rag_2_backend.Infrastructure.Database;
using rag_2_backend.Infrastructure.Database.Entity;
using rag_2_backend.Infrastructure.Module.Email;
using rag_2_backend.Infrastructure.Module.User.Dto;
using rag_2_backend.Infrastructure.Util;

#endregion

namespace rag_2_backend.Infrastructure.Module.User;

public class UserService(
    DatabaseContext context,
    EmailService emailService,
    UserDao userDao,
    RefreshTokenDao refreshTokenDao,
    CourseDao courseDao)
{
    public void RegisterUser(UserRequest userRequest)
    {
        if (context.Users.Any(u => u.Email == userRequest.Email))
            throw new BadRequestException("User already exists");

        Database.Entity.User user = new(userRequest.Email)
        {
            Name = userRequest.Name,
            Password = HashUtil.HashPassword(userRequest.Password)
        };

        if (user.Role == Role.Student)
            CheckStudentData(
                userRequest.StudyCycleYearA,
                userRequest.StudyCycleYearB,
                userRequest.CourseId,
                userRequest.Group
            );

        UpdateUserProperties(userRequest.StudyCycleYearA,
            userRequest.StudyCycleYearB,
            userRequest.CourseId,
            userRequest.Group,
            user
        );

        context.Users.Add(user);
        GenerateAccountTokenAndSendConfirmationMail(user);
        context.SaveChanges();
    }

    public void ResendConfirmationEmail(string email)
    {
        var user = userDao.GetUserByEmailOrThrow(email);
        if (user.Confirmed) throw new BadRequestException("User is already confirmed");

        context.AccountConfirmationTokens.RemoveRange(
            context.AccountConfirmationTokens.Where(a => a.User.Email == user.Email)
        );

        GenerateAccountTokenAndSendConfirmationMail(user);
        context.SaveChanges();
    }

    public void ConfirmAccount(string tokenValue)
    {
        var token = context.AccountConfirmationTokens
                        .Include(t => t.User)
                        .SingleOrDefault(t => t.Token == tokenValue)
                    ?? throw new BadRequestException("Invalid token");

        if (token.Expiration < DateTime.Now) throw new BadRequestException("Invalid token");

        token.User.Confirmed = true;
        context.AccountConfirmationTokens.Remove(token);
        context.SaveChanges();
    }

    public void UpdateAccount(UserEditRequest request, string principalEmail)
    {
        var user = userDao.GetUserByEmailOrThrow(principalEmail);

        if (user.Role == Role.Student)
            CheckStudentData(
                request.StudyCycleYearA,
                request.StudyCycleYearB,
                request.CourseId,
                request.Group
            );

        UpdateUserProperties(request.StudyCycleYearA,
            request.StudyCycleYearB,
            request.CourseId,
            request.Group,
            user
        );
        user.Name = request.Name;

        context.Users.Update(user);
        context.SaveChanges();
    }

    public void RequestPasswordReset(string email)
    {
        var user = userDao.GetUserByEmailOrThrow(email);

        context.PasswordResetTokens.RemoveRange(
            context.PasswordResetTokens.Where(a => a.User.Email == user.Email)
        );

        GeneratePasswordResetTokenAndSendMail(user);
        context.SaveChanges();
    }

    public void ResetPassword(string tokenValue, string newPassword)
    {
        var token = context.PasswordResetTokens
                        .Include(t => t.User)
                        .SingleOrDefault(t => t.Token == tokenValue)
                    ?? throw new BadRequestException("Invalid token");

        if (token.Expiration < DateTime.Now) throw new BadRequestException("Invalid token");

        token.User.Password = HashUtil.HashPassword(newPassword);
        context.PasswordResetTokens.Remove(token);
        context.SaveChanges();
    }

    public void ChangePassword(string email, string oldPassword, string newPassword)
    {
        var user = userDao.GetUserByEmailOrThrow(email);

        if (!HashUtil.VerifyPassword(oldPassword, user.Password))
            throw new BadRequestException("Invalid old password");
        if (user.Password == HashUtil.HashPassword(newPassword))
            throw new BadRequestException("Password cannot be same");

        user.Password = HashUtil.HashPassword(newPassword);
        context.SaveChanges();
    }

    public void DeleteAccount(string email, string header)
    {
        var user = userDao.GetUserByEmailOrThrow(email);

        context.PasswordResetTokens.RemoveRange(context.PasswordResetTokens
            .Include(p => p.User)
            .Where(a => a.User.Email == email)
        );
        context.AccountConfirmationTokens.RemoveRange(context.AccountConfirmationTokens
            .Include(p => p.User)
            .Where(a => a.User.Email == email)
        );
        context.GameRecords.RemoveRange(context.GameRecords
            .Include(p => p.User)
            .Where(a => a.User.Email == email)
        );

        context.Users.Remove(user);
        context.SaveChanges();

        refreshTokenDao.RemoveTokensForUser(user);
    }

    //

    private void UpdateUserProperties(
        int? studyCycleYearA, int? studyCycleYearB, int? courseId, string? group, Database.Entity.User user
    )
    {
        user.StudyCycleYearA = studyCycleYearA ?? 0;
        user.StudyCycleYearB = studyCycleYearB ?? 0;
        user.Course = courseId != null ? courseDao.GetCourseByIdOrThrow(courseId.Value) : null;
        user.Group = group;
    }

    private static void CheckStudentData(int? studyCycleYearA, int? studyCycleYearB, int? courseId, string? group)
    {
        if (!studyCycleYearA.HasValue || !studyCycleYearB.HasValue ||
            studyCycleYearA == 0 || studyCycleYearB == 0 ||
            studyCycleYearB - studyCycleYearA != 1)
            throw new BadRequestException("Wrong study cycle year");

        if (!courseId.HasValue) throw new BadRequestException("Wrong course id");

        if (string.IsNullOrEmpty(group)) throw new BadRequestException("Wrong group");
    }

    private void GenerateAccountTokenAndSendConfirmationMail(Database.Entity.User user)
    {
        var token = new AccountConfirmationToken
        {
            Token = Guid.NewGuid().ToString(),
            User = user,
            Expiration = DateTime.Now.AddDays(7)
        };
        context.AccountConfirmationTokens.Add(token);
        emailService.SendConfirmationEmail(user.Email, token.Token);
    }

    private void GeneratePasswordResetTokenAndSendMail(Database.Entity.User user)
    {
        var token = new PasswordResetToken
        {
            Token = Guid.NewGuid().ToString(),
            User = user,
            Expiration = DateTime.Now.AddDays(2)
        };
        context.PasswordResetTokens.Add(token);
        emailService.SendPasswordResetMail(user.Email, token.Token);
    }
}