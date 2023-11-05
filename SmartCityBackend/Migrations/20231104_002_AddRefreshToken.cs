using FluentMigrator;
namespace SmartCityBackend.Migrations;

[Migration(4)]
public class AddRefreshToken : Migration
{
    public override void Up()
    {
        Create.Table("refresh_token")
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn("created_at_utc").AsDateTimeOffset().NotNullable()
            .WithColumn("user_id").AsInt64().NotNullable().ForeignKey("FK_refresh_token_user_id", "users", "id")
            .WithColumn("token").AsString().NotNullable()
            .WithColumn("expires_at_utc").AsDateTimeOffset().NotNullable();
    }

    public override void Down()
    {
        Delete.Table("refresh_token");
    }
}
    
