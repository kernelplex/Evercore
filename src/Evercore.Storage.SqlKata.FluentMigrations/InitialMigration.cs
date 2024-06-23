using Evercore.Data;
using FluentMigrator;

namespace Evercore.Storage.SqlKata.FluentMigrations;

[Migration(20240621)]
public class InitialMigration : Migration
{
    public override void Up()
    {
        // ==========================================================
        // Type Lookup Tables
        // ==========================================================
        Create.Table(Tables.AgentTypes)
            .WithColumn("Id")
                .AsInt32()
                .NotNullable()
                .Identity()
                .PrimaryKey()
            .WithColumn("Name")
                .AsString()
                .NotNullable()
                .Unique();
        
        Create.Table(Tables.AggregateTypes)
            .WithColumn("Id")
            .AsInt32()
            .NotNullable()
            .Identity()
            .PrimaryKey()
            .WithColumn("Name")
            .AsString()
            .NotNullable()
            .Unique();
        
        Create.Table(Tables.EventTypes)
            .WithColumn("Id")
                .AsInt32()
                .NotNullable()
                .Identity()
                .PrimaryKey()
            .WithColumn("Name")
                .AsString()
                .NotNullable()
                .Unique();

        // ==========================================================
        // Agents Table
        // ==========================================================
        Create.Table(Tables.Agents)
            .WithColumn("Id")
                .AsInt32()
                .NotNullable()
                .Identity()
                .PrimaryKey()
            .WithColumn("AgentTypeId")
                .AsInt32()
                .NotNullable()
                .ForeignKey("FkAgentName", Tables.AgentTypes, "Id")
            .WithColumn("SystemId")
                .AsInt64()
                .Nullable()
            .WithColumn("AgentKey")
                .AsString(Limitations.MaximumSystemNameLength)
                .Nullable();
        
        Create.UniqueConstraint("UQ_AgentsAgentType_AgentKeySystemId")
            .OnTable(Tables.Agents)
            .Columns("AgentTypeId", "AgentKey", "SystemId");
        
        // ==========================================================
        // Aggregates Table
        // ==========================================================
        Create.Table(Tables.Aggregates)
            .WithColumn("Id")
                .AsInt64()
                .NotNullable()
                .Identity()
                .PrimaryKey()

            .WithColumn("AggregateTypeId")
                .AsInt32()
                .NotNullable()
                .ForeignKey(Tables.AggregateTypes, "Id")
            .WithColumn("NaturalKey")
                .AsString(Limitations.MaximumSystemNameLength)
                .Nullable()
                .Unique()
            .WithColumn("Sequence")
                .AsInt64()
                .NotNullable();

        // ==========================================================
        // AggregateEvents Table
        // ==========================================================
        Create.Table(Tables.AggregateEvents)
            .WithColumn("Id")
                .AsInt64()
                .NotNullable()
                .Identity()
                .PrimaryKey()
            .WithColumn("AggregateTypeId")
                .AsInt32()
                .NotNullable()
                .ForeignKey(Tables.AggregateTypes, "Id")
            .WithColumn("AggregateId")
                .AsInt64()
                .NotNullable()
                .ForeignKey(Tables.Aggregates, "Id")
            .WithColumn("EventTypeId")
                .AsInt32()
                .NotNullable()
                .ForeignKey(Tables.EventTypes, "Id")
            .WithColumn("Sequence")
                .AsInt64()
                .NotNullable()
            .WithColumn("Data")
                .AsString()
                .NotNullable()
            .WithColumn("AgentId")
            .AsInt32()
            .NotNullable()
            .WithColumn("EventTime")
                .AsDateTime2()
                .NotNullable();

        Create.UniqueConstraint("UQ_AggregateEvents_AggregateId_Sequence")
            .OnTable(Tables.AggregateEvents)
            .Columns("AggregateId", "Sequence");

        Create.Table(Tables.Snapshots)
            .WithColumn("Id")
                .AsInt64()
                .NotNullable()
                .Identity()
                .PrimaryKey()
            .WithColumn("AggregateTypeId")
                .AsInt32()
                .NotNullable()
                .ForeignKey(Tables.AggregateTypes, "Id")
                .WithColumn("AggregateId")
                .AsInt64()
                .NotNullable()
                .ForeignKey(Tables.Aggregates, "Id")
            .WithColumn("Sequence")
                .AsInt64()
                .NotNullable()
            .WithColumn("Version")
                .AsInt32()
                .NotNullable()
            .WithColumn("State")
                .AsString()
                .NotNullable();

        Create.Index("IX_Snapshots_AggregateId_Sequence")
            .OnTable(Tables.Snapshots)
            .OnColumn("AggregateId").Ascending()
            .OnColumn("Sequence").Descending();
    }

    public override void Down()
    {
        Delete.Table(Tables.Snapshots);
        Delete.Table(Tables.AggregateEvents);
        Delete.Table(Tables.EventTypes);
        Delete.Table(Tables.Aggregates);
        Delete.Table(Tables.AggregateTypes);
        Delete.Table(Tables.Agents);
        Delete.Table(Tables.AgentTypes);
    }
}