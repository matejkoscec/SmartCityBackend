using FluentMigrator;

namespace SmartCityBackend.Migrations;

[Migration(2)]
public class AddReservationsAndParkingSpotAndZonePrice : Migration {
    
    public override void Up()
    {
        Create.Table("parking_spot")
            .WithColumn("id").AsGuid().PrimaryKey().NotNullable()
            .WithColumn("created_at_utc").AsDateTimeOffset().NotNullable()
            .WithColumn("lat").AsDecimal().NotNullable()
            .WithColumn("lng").AsDecimal().NotNullable()
            .WithColumn("zone").AsInt32().NotNullable();

        Create.Table("active_reservation")
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn("created_at_utc").AsDateTimeOffset().NotNullable()
            .WithColumn("user_id").AsInt64().NotNullable().ForeignKey("FK_active_reservation_user_id", "users", "id")
            .WithColumn("start").AsDateTimeOffset().Nullable()
            .WithColumn("end").AsDateTimeOffset().NotNullable()
            .WithColumn("parking_spot_id").AsGuid().NotNullable().ForeignKey("FK_active_reservation_parking_spot_id", "parking_spot", "id");

        Create.Table("reservation_history")
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn("created_at_utc").AsDateTimeOffset().NotNullable()
            .WithColumn("user_id").AsInt64().NotNullable().ForeignKey("FK_reservation_history_user_id", "users", "id")
            .WithColumn("start").AsDateTimeOffset().Nullable()
            .WithColumn("end").AsDateTimeOffset().NotNullable()
            .WithColumn("parking_spot_id").AsGuid().NotNullable().ForeignKey("FK_reservation_history_parking_spot_id", "parking_spot", "id");
        
        Create.Table("zone_price")
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn("created_at_utc").AsDateTimeOffset().NotNullable()
            .WithColumn("zone").AsInt32().NotNullable()
            .WithColumn("price").AsDecimal().NotNullable();

        Create.Table("parking_spot_history")
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn("created_at_utc").AsDateTimeOffset().NotNullable()
            .WithColumn("parking_spot_id").AsGuid().NotNullable()
            .ForeignKey("FK_parking_spot_history_parking_spot_id", "parking_spot", "id")
            .WithColumn("active_reservation_id").AsInt64().Nullable()
            .ForeignKey("FK_parking_spot_history_active_reservation_id", "active_reservation", "id")
            .WithColumn("reservation_history_id").AsInt64().Nullable()
            .ForeignKey("FK_parking_spot_history_reservation_history_id", "reservation_history", "id")
            .WithColumn("zone_price_id").AsInt64().Nullable()
            .ForeignKey("FK_parking_spot_history_zone_price_id", "zone_price", "id")
            .WithColumn("start_time").AsDateTimeOffset().NotNullable()
            .WithColumn("is_occupied").AsBoolean().NotNullable();
    }

    public override void Down()
    {
        Delete.Table("zone_price");
        Delete.Table("parking_spot_history");
        Delete.Table("parking_spot");
        Delete.Table("reservation_history");
        Delete.Table("active_reservation");
    }
}