using FluentMigrator;

namespace SmartCityBackend.Migrations;

[Migration(1)]
public class AddUsersAndRoles : Migration {

    public override void Up()
    {
        Create.Table("users")
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn("created_at_utc").AsDateTimeOffset().NotNullable()
            .WithColumn("name").AsString().NotNullable()
            .WithColumn("email").AsString().NotNullable().Unique()
            .WithColumn("password").AsString().NotNullable()
            .WithColumn("preferred_username").AsString().NotNullable()
            .WithColumn("given_name").AsString().NotNullable()
            .WithColumn("family_name").AsString().NotNullable()
            .WithColumn("email_verified").AsBoolean().NotNullable();

        Create.Table("role")
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn("created_at_utc").AsDateTimeOffset().NotNullable()
            .WithColumn("name").AsString().NotNullable().Unique();

        Create.Table("user_role")
            .WithColumn("user_id").AsInt64().NotNullable().ForeignKey("FK_user_role_user_id", "users", "id")
            .WithColumn("role_id").AsInt64().NotNullable().ForeignKey("FK_user_role_role_id","role", "id");
    }

    public override void Down()
    {
        Delete.Table("user_role");
        Delete.Table("role");
        Delete.Table("users");
    }
}