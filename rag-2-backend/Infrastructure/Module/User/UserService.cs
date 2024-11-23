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
    public async Task RegisterUser(UserRequest userRequest)
    {
        if (await context.Users.AnyAsync(u => u.Email == userRequest.Email))
            throw new BadRequestException("User already exists");

        Database.Entity.User user = new(userRequest.Email)
        {
            Name = userRequest.Name,
            Password = HashUtil.HashPassword(userRequest.Password)
        };

        await UpdateUserProperties(
            userRequest.StudyCycleYearA,
            userRequest.StudyCycleYearB,
            userRequest.CourseId,
            userRequest.Group,
            user
        );

        await context.Users.AddAsync(user);
        await GenerateAccountTokenAndSendConfirmationMail(user);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAccount(UserEditRequest request, string principalEmail)
    {
        var user = await userDao.GetUserByEmailOrThrow(principalEmail);

        await UpdateUserProperties(
            request.StudyCycleYearA,
            request.StudyCycleYearB,
            request.CourseId,
            request.Group,
            user
        );
        user.Name = request.Name;

        context.Users.Update(user);
        await context.SaveChangesAsync();
    }

    public async Task ResendConfirmationEmail(string email)
    {
        var user = await userDao.GetUserByEmailOrThrow(email);
        if (user.Confirmed) throw new BadRequestException("User is already confirmed");

        context.AccountConfirmationTokens.RemoveRange(
            context.AccountConfirmationTokens.Where(a => a.User.Email == user.Email)
        );

        await GenerateAccountTokenAndSendConfirmationMail(user);
        await context.SaveChangesAsync();
    }

    public async Task ConfirmAccount(string tokenValue)
    {
        var token = await context.AccountConfirmationTokens
                        .Include(t => t.User)
                        .SingleOrDefaultAsync(t => t.Token == tokenValue)
                    ?? throw new BadRequestException("Invalid token");

        if (token.Expiration < DateTime.Now) throw new BadRequestException("Invalid token");

        token.User.Confirmed = true;
        context.AccountConfirmationTokens.Remove(token);
        await context.SaveChangesAsync();
    }

    public async Task RequestPasswordReset(string email)
    {
        var user = await userDao.GetUserByEmailOrThrow(email);

        context.PasswordResetTokens.RemoveRange(
            context.PasswordResetTokens.Where(a => a.User.Email == user.Email)
        );

        await GeneratePasswordResetTokenAndSendMail(user);
        await context.SaveChangesAsync();
    }

    public async Task ResetPassword(string tokenValue, string newPassword)
    {
        var token = await context.PasswordResetTokens
                        .Include(t => t.User)
                        .SingleOrDefaultAsync(t => t.Token == tokenValue)
                    ?? throw new BadRequestException("Invalid token");

        if (token.Expiration < DateTime.Now) throw new BadRequestException("Invalid token");

        token.User.Password = HashUtil.HashPassword(newPassword);
        context.PasswordResetTokens.Remove(token);
        await context.SaveChangesAsync();
    }

    public async Task ChangePassword(string email, string oldPassword, string newPassword)
    {
        var user = await userDao.GetUserByEmailOrThrow(email);

        if (!HashUtil.VerifyPassword(oldPassword, user.Password))
            throw new BadRequestException("Invalid old password");
        if (user.Password == HashUtil.HashPassword(newPassword))
            throw new BadRequestException("Password cannot be same");

        user.Password = HashUtil.HashPassword(newPassword);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAccount(string email)
    {
        var user = await userDao.GetUserByEmailOrThrow(email);

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
        await context.SaveChangesAsync();
        await refreshTokenDao.RemoveTokensForUser(user);
    }

    //

    private async Task UpdateUserProperties(
        int? studyCycleYearA, int? studyCycleYearB, int? courseId, string? group, Database.Entity.User user
    )
    {
        if (user.Role == Role.Student && (
                !studyCycleYearA.HasValue || !studyCycleYearB.HasValue || !courseId.HasValue ||
                string.IsNullOrWhiteSpace(group))
           ) throw new BadRequestException("Invalid data");

        if ((studyCycleYearA.HasValue && !studyCycleYearB.HasValue) ||
            (!studyCycleYearA.HasValue && studyCycleYearB.HasValue))
            throw new BadRequestException("Invalid data");

        if (studyCycleYearA.HasValue && studyCycleYearB.HasValue &&
            !IsStudyCycleYearValid(studyCycleYearA.Value, studyCycleYearB.Value))
            throw new BadRequestException("Invalid data");

        user.StudyCycleYearA = studyCycleYearA ?? 0;
        user.StudyCycleYearB = studyCycleYearB ?? 0;
        user.Course = courseId != null ? await courseDao.GetCourseByIdOrThrow(courseId.Value) : null;
        user.Group = group;
    }

    private static bool IsStudyCycleYearValid(int studyCycleYearA, int studyCycleYearB)
    {
        return studyCycleYearA != 0 && studyCycleYearB != 0 &&
               studyCycleYearB - studyCycleYearA == 1;
    }

    private async Task GenerateAccountTokenAndSendConfirmationMail(Database.Entity.User user)
    {
        var token = new AccountConfirmationToken
        {
            Token = Guid.NewGuid().ToString(),
            User = user,
            Expiration = DateTime.Now.AddDays(7)
        };
        await context.AccountConfirmationTokens.AddAsync(token);
        emailService.SendConfirmationEmail(user.Email, token.Token);
    }

    private async Task GeneratePasswordResetTokenAndSendMail(Database.Entity.User user)
    {
        var token = new PasswordResetToken
        {
            Token = Guid.NewGuid().ToString(),
            User = user,
            Expiration = DateTime.Now.AddDays(2)
        };
        await context.PasswordResetTokens.AddAsync(token);
        emailService.SendPasswordResetMail(user.Email, token.Token);
    }
}