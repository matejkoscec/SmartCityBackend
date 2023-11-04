using FluentMigrator;

namespace SmartCityBackend.Migrations;

[Migration (3)]
public class AddEventHubInfo : Migration
{
    public override void Up()
    {
        Create.Table("event_hub_info")
            .WithColumn ("id").AsInt64().PrimaryKey().Identity()
            .WithColumn ("created_at_utc").AsDateTimeOffset().NotNullable()
            .WithColumn ("offset").AsInt64().NotNullable();
    }

    public override void Down()
    {
        Delete.Table("event_hub_info");
    }    
}