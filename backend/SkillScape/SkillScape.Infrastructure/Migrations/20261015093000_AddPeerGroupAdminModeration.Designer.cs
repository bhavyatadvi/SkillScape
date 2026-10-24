using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SkillScape.Infrastructure.Data;

#nullable disable

namespace SkillScape.Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20261015093000_AddPeerGroupAdminModeration")]
    public partial class AddPeerGroupAdminModeration
    {
    }
}
