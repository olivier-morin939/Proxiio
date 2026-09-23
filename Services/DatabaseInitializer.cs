using Entities;
using Entities.Contexts;
using Entities.Enums;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;

namespace Services;

public class DatabaseInitializer(UsersDbContext db) : IDatabaseInitializer
{
    /// <summary>
    ///  Initialize some mock data for the first use od the database
    /// </summary>
    public void Initialize()
    {
        db.Database.EnsureCreated();

        if (db.Users.Any()) 
            return;


        User teacher = new User { UserId = Guid.NewGuid(), Name = "Camille Martin", Email = "camille@proxiio.ca", Role = Role.Moderator, UserState = UserState.Active };
        User learner = new User { UserId = Guid.NewGuid(), Name = "Alex Tremblay", Email = "alex@proxiio.ca", Role = Role.User, UserState = UserState.Active };
        db.Users.AddRange(teacher, learner);

        Comunity design = new Comunity { Id = Guid.NewGuid(), TeacherId = teacher.UserId, Name = "Créateurs & Design", Description = "Une communauté pour apprendre le design produit, partager ses projets et progresser ensemble." };
        Comunity dev = new Comunity { Id = Guid.NewGuid(), TeacherId = teacher.UserId, Name = "Cercle des développeurs", Description = "Entraide, projets concrets et bonnes pratiques pour les développeurs web." };
        db.Comunities.AddRange(design, dev);

        db.ComunityMembers.AddRange(
            new ComunityMember { ComunityId = design.Id, UserId = teacher.UserId, Role = ComunityRole.Teacher },
            new ComunityMember { ComunityId = design.Id, UserId = learner.UserId },
            new ComunityMember { ComunityId = dev.Id, UserId = teacher.UserId, Role = ComunityRole.Teacher });
        DateTime now = DateTime.UtcNow;

        db.Posts.AddRange(
            new Post { Id = Guid.NewGuid(), CommunityId = design.Id, UserId = teacher.UserId, Title = "Bienvenue dans la communauté 👋", Body = "Présentez-vous dans les commentaires : sur quoi travaillez-vous en ce moment et qu’aimeriez-vous apprendre ?", CreatedAt = now.AddHours(-2), ModifiedAt = now.AddHours(-2) },
            new Post { Id = Guid.NewGuid(), CommunityId = design.Id, UserId = learner.UserId, Title = "Mon premier projet de refonte", Body = "Je commence une refonte de mon portfolio cette semaine. Vos conseils pour bien démarrer sur Figma sont les bienvenus !", CreatedAt = now.AddMinutes(-48), ModifiedAt = now.AddMinutes(-48) },
            new Post { Id = Guid.NewGuid(), CommunityId = dev.Id, UserId = teacher.UserId, Title = "Défi de la semaine : construisez une API", Body = "Cette semaine, on construit une petite API, on partage nos solutions et on s’entraide dans les commentaires.", CreatedAt = now.AddDays(-1), ModifiedAt = now.AddDays(-1) });
        db.SaveChanges();
    }
}
