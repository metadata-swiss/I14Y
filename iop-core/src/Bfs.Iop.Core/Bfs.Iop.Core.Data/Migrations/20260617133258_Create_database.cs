using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bfs.Iop.Core.Data.Migrations
{
    /// <inheritdoc />
    public partial class Create_database : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "data");

            migrationBuilder.CreateTable(
                name: "agent",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    classification = table.Column<string>(type: "text", nullable: true),
                    description_de = table.Column<string>(type: "text", nullable: true),
                    description_en = table.Column<string>(type: "text", nullable: true),
                    description_fr = table.Column<string>(type: "text", nullable: true),
                    description_it = table.Column<string>(type: "text", nullable: true),
                    description_rm = table.Column<string>(type: "text", nullable: true),
                    home_page = table.Column<string>(type: "text", nullable: true),
                    identifier = table.Column<string>(type: "text", nullable: false),
                    name_de = table.Column<string>(type: "text", nullable: true),
                    name_en = table.Column<string>(type: "text", nullable: true),
                    name_fr = table.Column<string>(type: "text", nullable: true),
                    name_it = table.Column<string>(type: "text", nullable: true),
                    name_rm = table.Column<string>(type: "text", nullable: true),
                    pref_label_de = table.Column<string>(type: "text", nullable: true),
                    pref_label_en = table.Column<string>(type: "text", nullable: true),
                    pref_label_fr = table.Column<string>(type: "text", nullable: true),
                    pref_label_it = table.Column<string>(type: "text", nullable: true),
                    pref_label_rm = table.Column<string>(type: "text", nullable: true),
                    spatial = table.Column<string[]>(type: "text[]", nullable: false),
                    spatial_ch = table.Column<string[]>(type: "text[]", nullable: false),
                    uid = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creation_type = table.Column<int>(type: "integer", nullable: true),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_agent", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "dataset_quality_question",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    mandatory = table.Column<bool>(type: "boolean", nullable: false),
                    order = table.Column<int>(type: "integer", nullable: false),
                    question_de = table.Column<string>(type: "text", nullable: true),
                    question_en = table.Column<string>(type: "text", nullable: true),
                    question_fr = table.Column<string>(type: "text", nullable: true),
                    question_it = table.Column<string>(type: "text", nullable: true),
                    question_rm = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creation_type = table.Column<int>(type: "integer", nullable: true),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dataset_quality_question", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "iop_persons",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    given_name = table.Column<string>(type: "text", nullable: false),
                    family_name = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    first_login_date = table.Column<DateOnly>(type: "date", nullable: false),
                    last_login_date = table.Column<DateOnly>(type: "date", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creation_type = table.Column<int>(type: "integer", nullable: true),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_iop_persons", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "vocabulary_config",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    vocabulary_identifier = table.Column<string>(type: "text", nullable: false),
                    concept_identifier = table.Column<string>(type: "text", nullable: false),
                    concept_version = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creation_type = table.Column<int>(type: "integer", nullable: true),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vocabulary_config", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "agent_sub_agent_relation",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    agent_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sub_agent_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creation_type = table.Column<int>(type: "integer", nullable: true),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_agent_sub_agent_relation", x => x.id);
                    table.ForeignKey(
                        name: "fk_agent_sub_agent_relation_agent_agent_id",
                        column: x => x.agent_id,
                        principalSchema: "data",
                        principalTable: "agent",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_agent_sub_agent_relation_agent_sub_agent_id",
                        column: x => x.sub_agent_id,
                        principalSchema: "data",
                        principalTable: "agent",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "dcat_catalog",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    description_de = table.Column<string>(type: "text", nullable: true),
                    description_en = table.Column<string>(type: "text", nullable: true),
                    description_fr = table.Column<string>(type: "text", nullable: true),
                    description_it = table.Column<string>(type: "text", nullable: true),
                    description_rm = table.Column<string>(type: "text", nullable: true),
                    title_de = table.Column<string>(type: "text", nullable: true),
                    title_en = table.Column<string>(type: "text", nullable: true),
                    title_fr = table.Column<string>(type: "text", nullable: true),
                    title_it = table.Column<string>(type: "text", nullable: true),
                    title_rm = table.Column<string>(type: "text", nullable: true),
                    publisher_id = table.Column<Guid>(type: "uuid", nullable: false),
                    theme_taxonomy = table.Column<string[]>(type: "text[]", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creation_type = table.Column<int>(type: "integer", nullable: true),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dcat_catalog", x => x.id);
                    table.ForeignKey(
                        name: "fk_dcat_catalog_agent_publisher_id",
                        column: x => x.publisher_id,
                        principalSchema: "data",
                        principalTable: "agent",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "dataset_quality_answer_option",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    dataset_quality_question_id = table.Column<Guid>(type: "uuid", nullable: false),
                    detail_de = table.Column<string>(type: "text", nullable: true),
                    detail_en = table.Column<string>(type: "text", nullable: true),
                    detail_fr = table.Column<string>(type: "text", nullable: true),
                    detail_it = table.Column<string>(type: "text", nullable: true),
                    detail_rm = table.Column<string>(type: "text", nullable: true),
                    detail_mandatory = table.Column<bool>(type: "boolean", nullable: false),
                    name_de = table.Column<string>(type: "text", nullable: true),
                    name_en = table.Column<string>(type: "text", nullable: true),
                    name_fr = table.Column<string>(type: "text", nullable: true),
                    name_it = table.Column<string>(type: "text", nullable: true),
                    name_rm = table.Column<string>(type: "text", nullable: true),
                    value = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creation_type = table.Column<int>(type: "integer", nullable: true),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dataset_quality_answer_option", x => x.id);
                    table.ForeignKey(
                        name: "fk_dataset_quality_answer_option_dataset_quality_question_data~",
                        column: x => x.dataset_quality_question_id,
                        principalSchema: "data",
                        principalTable: "dataset_quality_question",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "data_service",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    access_rights = table.Column<string>(type: "text", nullable: false),
                    description_de = table.Column<string>(type: "text", nullable: true),
                    description_en = table.Column<string>(type: "text", nullable: true),
                    description_fr = table.Column<string>(type: "text", nullable: true),
                    description_it = table.Column<string>(type: "text", nullable: true),
                    description_rm = table.Column<string>(type: "text", nullable: true),
                    identifiers = table.Column<string[]>(type: "text[]", nullable: false),
                    issued = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    license = table.Column<string>(type: "text", nullable: true),
                    modified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    responsible_person_id = table.Column<Guid>(type: "uuid", nullable: true),
                    responsible_deputy_id = table.Column<Guid>(type: "uuid", nullable: true),
                    theme = table.Column<string[]>(type: "text[]", nullable: false),
                    title_de = table.Column<string>(type: "text", nullable: true),
                    title_en = table.Column<string>(type: "text", nullable: true),
                    title_fr = table.Column<string>(type: "text", nullable: true),
                    title_it = table.Column<string>(type: "text", nullable: true),
                    title_rm = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creation_type = table.Column<int>(type: "integer", nullable: true),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    publication_level = table.Column<int>(type: "integer", nullable: false),
                    publication_level_proposal = table.Column<int>(type: "integer", nullable: true),
                    publisher_id = table.Column<Guid>(type: "uuid", nullable: false),
                    registration_status = table.Column<int>(type: "integer", nullable: false),
                    registration_status_proposal = table.Column<int>(type: "integer", nullable: true),
                    previous_version_id = table.Column<Guid>(type: "uuid", nullable: true),
                    version = table.Column<string>(type: "text", nullable: true),
                    version_notes_de = table.Column<string>(type: "text", nullable: true),
                    version_notes_en = table.Column<string>(type: "text", nullable: true),
                    version_notes_fr = table.Column<string>(type: "text", nullable: true),
                    version_notes_it = table.Column<string>(type: "text", nullable: true),
                    version_notes_rm = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_data_service", x => x.id);
                    table.ForeignKey(
                        name: "fk_data_service_agent_publisher_id",
                        column: x => x.publisher_id,
                        principalSchema: "data",
                        principalTable: "agent",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_data_service_data_service_previous_version_id",
                        column: x => x.previous_version_id,
                        principalSchema: "data",
                        principalTable: "data_service",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_data_service_iop_persons_responsible_deputy_id",
                        column: x => x.responsible_deputy_id,
                        principalSchema: "data",
                        principalTable: "iop_persons",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_data_service_iop_persons_responsible_person_id",
                        column: x => x.responsible_person_id,
                        principalSchema: "data",
                        principalTable: "iop_persons",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "dataset",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    access_rights = table.Column<string>(type: "text", nullable: false),
                    confidentiality_person = table.Column<string>(type: "text", nullable: true),
                    data_owner = table.Column<string>(type: "text", nullable: true),
                    description_de = table.Column<string>(type: "text", nullable: true),
                    description_en = table.Column<string>(type: "text", nullable: true),
                    description_fr = table.Column<string>(type: "text", nullable: true),
                    description_it = table.Column<string>(type: "text", nullable: true),
                    description_rm = table.Column<string>(type: "text", nullable: true),
                    frequency = table.Column<string>(type: "text", nullable: true),
                    geo_iv_id = table.Column<string[]>(type: "text[]", nullable: false),
                    identifier = table.Column<string[]>(type: "text[]", nullable: false),
                    issued = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    language = table.Column<string[]>(type: "text[]", nullable: false),
                    modified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    other_identifier = table.Column<string[]>(type: "text[]", nullable: true),
                    process_id = table.Column<string>(type: "text", nullable: true),
                    qualified_attribution_complement_de = table.Column<string>(type: "text", nullable: true),
                    qualified_attribution_complement_en = table.Column<string>(type: "text", nullable: true),
                    qualified_attribution_complement_fr = table.Column<string>(type: "text", nullable: true),
                    qualified_attribution_complement_it = table.Column<string>(type: "text", nullable: true),
                    qualified_attribution_complement_rm = table.Column<string>(type: "text", nullable: true),
                    responsible_person_id = table.Column<Guid>(type: "uuid", nullable: true),
                    responsible_deputy_id = table.Column<Guid>(type: "uuid", nullable: true),
                    retention_period = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    retention_period_description_de = table.Column<string>(type: "text", nullable: true),
                    retention_period_description_en = table.Column<string>(type: "text", nullable: true),
                    retention_period_description_fr = table.Column<string>(type: "text", nullable: true),
                    retention_period_description_it = table.Column<string>(type: "text", nullable: true),
                    retention_period_description_rm = table.Column<string>(type: "text", nullable: true),
                    spatial = table.Column<string[]>(type: "text[]", nullable: true),
                    theme = table.Column<string[]>(type: "text[]", nullable: false),
                    title_de = table.Column<string>(type: "text", nullable: true),
                    title_en = table.Column<string>(type: "text", nullable: true),
                    title_fr = table.Column<string>(type: "text", nullable: true),
                    title_it = table.Column<string>(type: "text", nullable: true),
                    title_rm = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creation_type = table.Column<int>(type: "integer", nullable: true),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    publication_level = table.Column<int>(type: "integer", nullable: false),
                    publication_level_proposal = table.Column<int>(type: "integer", nullable: true),
                    publisher_id = table.Column<Guid>(type: "uuid", nullable: false),
                    registration_status = table.Column<int>(type: "integer", nullable: false),
                    registration_status_proposal = table.Column<int>(type: "integer", nullable: true),
                    previous_version_id = table.Column<Guid>(type: "uuid", nullable: true),
                    version = table.Column<string>(type: "text", nullable: true),
                    version_notes_de = table.Column<string>(type: "text", nullable: true),
                    version_notes_en = table.Column<string>(type: "text", nullable: true),
                    version_notes_fr = table.Column<string>(type: "text", nullable: true),
                    version_notes_it = table.Column<string>(type: "text", nullable: true),
                    version_notes_rm = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dataset", x => x.id);
                    table.ForeignKey(
                        name: "fk_dataset_agent_publisher_id",
                        column: x => x.publisher_id,
                        principalSchema: "data",
                        principalTable: "agent",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_dataset_dataset_previous_version_id",
                        column: x => x.previous_version_id,
                        principalSchema: "data",
                        principalTable: "dataset",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_dataset_iop_persons_responsible_deputy_id",
                        column: x => x.responsible_deputy_id,
                        principalSchema: "data",
                        principalTable: "iop_persons",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_dataset_iop_persons_responsible_person_id",
                        column: x => x.responsible_person_id,
                        principalSchema: "data",
                        principalTable: "iop_persons",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "iop_concepts",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code_list_entry_value_max_length = table.Column<int>(type: "integer", nullable: true),
                    code_list_entry_value_type = table.Column<int>(type: "integer", nullable: true),
                    code_list_entry_default_sort_property = table.Column<int>(type: "integer", nullable: true),
                    concept_type = table.Column<int>(type: "integer", nullable: false),
                    description_de = table.Column<string>(type: "text", nullable: true),
                    description_en = table.Column<string>(type: "text", nullable: true),
                    description_fr = table.Column<string>(type: "text", nullable: true),
                    description_it = table.Column<string>(type: "text", nullable: true),
                    description_rm = table.Column<string>(type: "text", nullable: true),
                    identifiers = table.Column<string[]>(type: "text[]", nullable: false),
                    is_locked = table.Column<bool>(type: "boolean", nullable: false),
                    max_length = table.Column<int>(type: "integer", nullable: true),
                    max_value = table.Column<decimal>(type: "numeric", nullable: true),
                    measurement_unit = table.Column<string>(type: "text", nullable: true),
                    min_length = table.Column<int>(type: "integer", nullable: true),
                    min_value = table.Column<decimal>(type: "numeric", nullable: true),
                    name_de = table.Column<string>(type: "text", nullable: true),
                    name_en = table.Column<string>(type: "text", nullable: true),
                    name_fr = table.Column<string>(type: "text", nullable: true),
                    name_it = table.Column<string>(type: "text", nullable: true),
                    name_rm = table.Column<string>(type: "text", nullable: true),
                    number_decimals = table.Column<int>(type: "integer", nullable: true),
                    pattern = table.Column<string>(type: "text", nullable: true),
                    responsible_deputy_id = table.Column<Guid>(type: "uuid", nullable: true),
                    responsible_person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    themes = table.Column<string[]>(type: "text[]", nullable: false),
                    valid_from = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    valid_to = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    version = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creation_type = table.Column<int>(type: "integer", nullable: true),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    publication_level = table.Column<int>(type: "integer", nullable: false),
                    publication_level_proposal = table.Column<int>(type: "integer", nullable: true),
                    publisher_id = table.Column<Guid>(type: "uuid", nullable: false),
                    registration_status = table.Column<int>(type: "integer", nullable: false),
                    registration_status_proposal = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_iop_concepts", x => x.id);
                    table.ForeignKey(
                        name: "fk_iop_concepts_agent_publisher_id",
                        column: x => x.publisher_id,
                        principalSchema: "data",
                        principalTable: "agent",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_iop_concepts_iop_persons_responsible_deputy_id",
                        column: x => x.responsible_deputy_id,
                        principalSchema: "data",
                        principalTable: "iop_persons",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_iop_concepts_iop_persons_responsible_person_id",
                        column: x => x.responsible_person_id,
                        principalSchema: "data",
                        principalTable: "iop_persons",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "mapping_tables",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    description_de = table.Column<string>(type: "text", nullable: true),
                    description_en = table.Column<string>(type: "text", nullable: true),
                    description_fr = table.Column<string>(type: "text", nullable: true),
                    description_it = table.Column<string>(type: "text", nullable: true),
                    description_rm = table.Column<string>(type: "text", nullable: true),
                    identifiers = table.Column<string[]>(type: "text[]", nullable: false),
                    name_de = table.Column<string>(type: "text", nullable: true),
                    name_en = table.Column<string>(type: "text", nullable: true),
                    name_fr = table.Column<string>(type: "text", nullable: true),
                    name_it = table.Column<string>(type: "text", nullable: true),
                    name_rm = table.Column<string>(type: "text", nullable: true),
                    responsible_deputy_id = table.Column<Guid>(type: "uuid", nullable: true),
                    responsible_person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_uri = table.Column<string>(type: "text", nullable: false),
                    target_uri = table.Column<string>(type: "text", nullable: false),
                    themes = table.Column<string[]>(type: "text[]", nullable: false),
                    valid_from = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    valid_to = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    version = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creation_type = table.Column<int>(type: "integer", nullable: true),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    publication_level = table.Column<int>(type: "integer", nullable: false),
                    publication_level_proposal = table.Column<int>(type: "integer", nullable: true),
                    publisher_id = table.Column<Guid>(type: "uuid", nullable: false),
                    registration_status = table.Column<int>(type: "integer", nullable: false),
                    registration_status_proposal = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mapping_tables", x => x.id);
                    table.ForeignKey(
                        name: "fk_mapping_tables_agent_publisher_id",
                        column: x => x.publisher_id,
                        principalSchema: "data",
                        principalTable: "agent",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_mapping_tables_iop_persons_responsible_deputy_id",
                        column: x => x.responsible_deputy_id,
                        principalSchema: "data",
                        principalTable: "iop_persons",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_mapping_tables_iop_persons_responsible_person_id",
                        column: x => x.responsible_person_id,
                        principalSchema: "data",
                        principalTable: "iop_persons",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "public_services",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    business_events = table.Column<string[]>(type: "text[]", nullable: false),
                    description_de = table.Column<string>(type: "text", nullable: true),
                    description_en = table.Column<string>(type: "text", nullable: true),
                    description_fr = table.Column<string>(type: "text", nullable: true),
                    description_it = table.Column<string>(type: "text", nullable: true),
                    description_rm = table.Column<string>(type: "text", nullable: true),
                    identifiers = table.Column<string[]>(type: "text[]", nullable: false),
                    language = table.Column<string[]>(type: "text[]", nullable: false),
                    life_events = table.Column<string[]>(type: "text[]", nullable: false),
                    responsible_deputy_id = table.Column<Guid>(type: "uuid", nullable: true),
                    responsible_person_id = table.Column<Guid>(type: "uuid", nullable: true),
                    sector = table.Column<string[]>(type: "text[]", nullable: false),
                    spatial = table.Column<string[]>(type: "text[]", nullable: false),
                    thematic_area = table.Column<string[]>(type: "text[]", nullable: false),
                    title_de = table.Column<string>(type: "text", nullable: true),
                    title_en = table.Column<string>(type: "text", nullable: true),
                    title_fr = table.Column<string>(type: "text", nullable: true),
                    title_it = table.Column<string>(type: "text", nullable: true),
                    title_rm = table.Column<string>(type: "text", nullable: true),
                    spatial_ch = table.Column<string[]>(type: "text[]", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creation_type = table.Column<int>(type: "integer", nullable: true),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    publication_level = table.Column<int>(type: "integer", nullable: false),
                    publication_level_proposal = table.Column<int>(type: "integer", nullable: true),
                    publisher_id = table.Column<Guid>(type: "uuid", nullable: false),
                    registration_status = table.Column<int>(type: "integer", nullable: false),
                    registration_status_proposal = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_public_services", x => x.id);
                    table.ForeignKey(
                        name: "fk_public_services_agent_publisher_id",
                        column: x => x.publisher_id,
                        principalSchema: "data",
                        principalTable: "agent",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_public_services_iop_persons_responsible_deputy_id",
                        column: x => x.responsible_deputy_id,
                        principalSchema: "data",
                        principalTable: "iop_persons",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_public_services_iop_persons_responsible_person_id",
                        column: x => x.responsible_person_id,
                        principalSchema: "data",
                        principalTable: "iop_persons",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "dcat_catalog_record",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    dcat_catalog_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creation_type = table.Column<int>(type: "integer", nullable: true),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dcat_catalog_record", x => x.id);
                    table.ForeignKey(
                        name: "fk_dcat_catalog_record_dcat_catalog_dcat_catalog_id",
                        column: x => x.dcat_catalog_id,
                        principalSchema: "data",
                        principalTable: "dcat_catalog",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "data_service_dataset",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_service_id = table.Column<Guid>(type: "uuid", nullable: false),
                    dataset_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creation_type = table.Column<int>(type: "integer", nullable: true),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_data_service_dataset", x => x.id);
                    table.ForeignKey(
                        name: "fk_data_service_dataset_data_service_data_service_id",
                        column: x => x.data_service_id,
                        principalSchema: "data",
                        principalTable: "data_service",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_data_service_dataset_dataset_dataset_id",
                        column: x => x.dataset_id,
                        principalSchema: "data",
                        principalTable: "dataset",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "dataset_quality_information",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    answer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    dataset_id = table.Column<Guid>(type: "uuid", nullable: false),
                    detail = table.Column<string>(type: "text", nullable: true),
                    question_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creation_type = table.Column<int>(type: "integer", nullable: true),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dataset_quality_information", x => x.id);
                    table.ForeignKey(
                        name: "fk_dataset_quality_information_dataset_dataset_id",
                        column: x => x.dataset_id,
                        principalSchema: "data",
                        principalTable: "dataset",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_dataset_quality_information_dataset_quality_answer_option_a~",
                        column: x => x.answer_id,
                        principalSchema: "data",
                        principalTable: "dataset_quality_answer_option",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_dataset_quality_information_dataset_quality_question_questi~",
                        column: x => x.question_id,
                        principalSchema: "data",
                        principalTable: "dataset_quality_question",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "dataset_quality_information_link",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    dataset_id = table.Column<Guid>(type: "uuid", nullable: false),
                    href = table.Column<string>(type: "text", nullable: false),
                    label_de = table.Column<string>(type: "text", nullable: true),
                    label_en = table.Column<string>(type: "text", nullable: true),
                    label_fr = table.Column<string>(type: "text", nullable: true),
                    label_it = table.Column<string>(type: "text", nullable: true),
                    label_rm = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creation_type = table.Column<int>(type: "integer", nullable: true),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dataset_quality_information_link", x => x.id);
                    table.ForeignKey(
                        name: "fk_dataset_quality_information_link_dataset_dataset_id",
                        column: x => x.dataset_id,
                        principalSchema: "data",
                        principalTable: "dataset",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "distribution",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    availability_vocabulary = table.Column<string>(type: "text", nullable: true),
                    byte_size = table.Column<decimal>(type: "numeric", nullable: true),
                    dataset_id = table.Column<Guid>(type: "uuid", nullable: true),
                    description_de = table.Column<string>(type: "text", nullable: true),
                    description_en = table.Column<string>(type: "text", nullable: true),
                    description_fr = table.Column<string>(type: "text", nullable: true),
                    description_it = table.Column<string>(type: "text", nullable: true),
                    description_rm = table.Column<string>(type: "text", nullable: true),
                    format = table.Column<string>(type: "text", nullable: true),
                    identifier = table.Column<string>(type: "text", nullable: true),
                    issued = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    language = table.Column<string[]>(type: "text[]", nullable: false),
                    license = table.Column<string>(type: "text", nullable: true),
                    media_type = table.Column<string>(type: "text", nullable: true),
                    modified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    packaging_format = table.Column<string>(type: "text", nullable: true),
                    rights = table.Column<string>(type: "text", nullable: true),
                    spatial_resolution = table.Column<string[]>(type: "text[]", nullable: false),
                    temporal_resolution = table.Column<string>(type: "text", nullable: true),
                    title_de = table.Column<string>(type: "text", nullable: true),
                    title_en = table.Column<string>(type: "text", nullable: true),
                    title_fr = table.Column<string>(type: "text", nullable: true),
                    title_it = table.Column<string>(type: "text", nullable: true),
                    title_rm = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creation_type = table.Column<int>(type: "integer", nullable: true),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_distribution", x => x.id);
                    table.ForeignKey(
                        name: "fk_distribution_dataset_dataset_id",
                        column: x => x.dataset_id,
                        principalSchema: "data",
                        principalTable: "dataset",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "qualified_attribution",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    agent_id = table.Column<Guid>(type: "uuid", nullable: false),
                    dataset_id = table.Column<Guid>(type: "uuid", nullable: true),
                    had_role = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creation_type = table.Column<int>(type: "integer", nullable: true),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_qualified_attribution", x => x.id);
                    table.ForeignKey(
                        name: "fk_qualified_attribution_agent_agent_id",
                        column: x => x.agent_id,
                        principalSchema: "data",
                        principalTable: "agent",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_qualified_attribution_dataset_dataset_id",
                        column: x => x.dataset_id,
                        principalSchema: "data",
                        principalTable: "dataset",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "qualified_relation",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    dataset_id = table.Column<Guid>(type: "uuid", nullable: true),
                    had_role = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creation_type = table.Column<int>(type: "integer", nullable: true),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_qualified_relation", x => x.id);
                    table.ForeignKey(
                        name: "fk_qualified_relation_dataset_dataset_id",
                        column: x => x.dataset_id,
                        principalSchema: "data",
                        principalTable: "dataset",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vcard",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    agent_contact_point_id = table.Column<Guid>(type: "uuid", nullable: true),
                    adr_work_de = table.Column<string>(type: "text", nullable: true),
                    adr_work_en = table.Column<string>(type: "text", nullable: true),
                    adr_work_fr = table.Column<string>(type: "text", nullable: true),
                    adr_work_it = table.Column<string>(type: "text", nullable: true),
                    adr_work_rm = table.Column<string>(type: "text", nullable: true),
                    child = table.Column<string>(type: "text", nullable: false),
                    data_service_contact_point_id = table.Column<Guid>(type: "uuid", nullable: true),
                    dataset_contact_point_id = table.Column<Guid>(type: "uuid", nullable: true),
                    email_internet = table.Column<string>(type: "text", nullable: true),
                    fn_de = table.Column<string>(type: "text", nullable: true),
                    fn_en = table.Column<string>(type: "text", nullable: true),
                    fn_fr = table.Column<string>(type: "text", nullable: true),
                    fn_it = table.Column<string>(type: "text", nullable: true),
                    fn_rm = table.Column<string>(type: "text", nullable: true),
                    note_de = table.Column<string>(type: "text", nullable: true),
                    note_en = table.Column<string>(type: "text", nullable: true),
                    note_fr = table.Column<string>(type: "text", nullable: true),
                    note_it = table.Column<string>(type: "text", nullable: true),
                    note_rm = table.Column<string>(type: "text", nullable: true),
                    org_de = table.Column<string>(type: "text", nullable: true),
                    org_en = table.Column<string>(type: "text", nullable: true),
                    org_fr = table.Column<string>(type: "text", nullable: true),
                    org_it = table.Column<string>(type: "text", nullable: true),
                    org_rm = table.Column<string>(type: "text", nullable: true),
                    tel_work_voice = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creation_type = table.Column<int>(type: "integer", nullable: true),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vcard", x => x.id);
                    table.ForeignKey(
                        name: "fk_vcard_agent_agent_contact_point_id",
                        column: x => x.agent_contact_point_id,
                        principalSchema: "data",
                        principalTable: "agent",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_vcard_data_service_data_service_contact_point_id",
                        column: x => x.data_service_contact_point_id,
                        principalSchema: "data",
                        principalTable: "data_service",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_vcard_dataset_dataset_contact_point_id",
                        column: x => x.dataset_contact_point_id,
                        principalSchema: "data",
                        principalTable: "dataset",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "code_list_entries",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    description_de = table.Column<string>(type: "text", nullable: true),
                    description_en = table.Column<string>(type: "text", nullable: true),
                    description_fr = table.Column<string>(type: "text", nullable: true),
                    description_it = table.Column<string>(type: "text", nullable: true),
                    description_rm = table.Column<string>(type: "text", nullable: true),
                    name_de = table.Column<string>(type: "text", nullable: true),
                    name_en = table.Column<string>(type: "text", nullable: true),
                    name_fr = table.Column<string>(type: "text", nullable: true),
                    name_it = table.Column<string>(type: "text", nullable: true),
                    name_rm = table.Column<string>(type: "text", nullable: true),
                    iop_concept_id = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_code_list_entry_id = table.Column<Guid>(type: "uuid", nullable: true),
                    position = table.Column<int>(type: "integer", nullable: false),
                    valid_from = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    valid_to = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creation_type = table.Column<int>(type: "integer", nullable: true),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_code_list_entries", x => x.id);
                    table.ForeignKey(
                        name: "fk_code_list_entries_code_list_entries_parent_code_list_entry_~",
                        column: x => x.parent_code_list_entry_id,
                        principalSchema: "data",
                        principalTable: "code_list_entries",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_code_list_entries_iop_concepts_iop_concept_id",
                        column: x => x.iop_concept_id,
                        principalSchema: "data",
                        principalTable: "iop_concepts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mapping_relations",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    mapping_table_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_code_uri = table.Column<string>(type: "text", nullable: false),
                    target_code_uri = table.Column<string>(type: "text", nullable: false),
                    relation_type = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creation_type = table.Column<int>(type: "integer", nullable: true),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mapping_relations", x => x.id);
                    table.ForeignKey(
                        name: "fk_mapping_relations_mapping_tables_mapping_table_id",
                        column: x => x.mapping_table_id,
                        principalSchema: "data",
                        principalTable: "mapping_tables",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "channels",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    address_de = table.Column<string>(type: "text", nullable: true),
                    address_en = table.Column<string>(type: "text", nullable: true),
                    address_fr = table.Column<string>(type: "text", nullable: true),
                    address_it = table.Column<string>(type: "text", nullable: true),
                    address_rm = table.Column<string>(type: "text", nullable: true),
                    description_de = table.Column<string>(type: "text", nullable: true),
                    description_en = table.Column<string>(type: "text", nullable: true),
                    description_fr = table.Column<string>(type: "text", nullable: true),
                    description_it = table.Column<string>(type: "text", nullable: true),
                    description_rm = table.Column<string>(type: "text", nullable: true),
                    email = table.Column<string>(type: "text", nullable: true),
                    fax = table.Column<string>(type: "text", nullable: true),
                    identifier = table.Column<string>(type: "text", nullable: false),
                    mobile = table.Column<string>(type: "text", nullable: true),
                    opening_hours = table.Column<string>(type: "text", nullable: true),
                    phone = table.Column<string>(type: "text", nullable: true),
                    public_service_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "text", nullable: true),
                    url = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creation_type = table.Column<int>(type: "integer", nullable: true),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_channels", x => x.id);
                    table.ForeignKey(
                        name: "fk_channels_public_services_public_service_id",
                        column: x => x.public_service_id,
                        principalSchema: "data",
                        principalTable: "public_services",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "keyword",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_service_id = table.Column<Guid>(type: "uuid", nullable: true),
                    dataset_id = table.Column<Guid>(type: "uuid", nullable: true),
                    public_service_id = table.Column<Guid>(type: "uuid", nullable: true),
                    iop_concept_id = table.Column<Guid>(type: "uuid", nullable: true),
                    mapping_table_id = table.Column<Guid>(type: "uuid", nullable: true),
                    text_de = table.Column<string>(type: "text", nullable: true),
                    text_en = table.Column<string>(type: "text", nullable: true),
                    text_fr = table.Column<string>(type: "text", nullable: true),
                    text_it = table.Column<string>(type: "text", nullable: true),
                    text_rm = table.Column<string>(type: "text", nullable: true),
                    uri = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creation_type = table.Column<int>(type: "integer", nullable: true),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_keyword", x => x.id);
                    table.ForeignKey(
                        name: "fk_keyword_data_service_data_service_id",
                        column: x => x.data_service_id,
                        principalSchema: "data",
                        principalTable: "data_service",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_keyword_dataset_dataset_id",
                        column: x => x.dataset_id,
                        principalSchema: "data",
                        principalTable: "dataset",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_keyword_iop_concepts_iop_concept_id",
                        column: x => x.iop_concept_id,
                        principalSchema: "data",
                        principalTable: "iop_concepts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_keyword_mapping_tables_mapping_table_id",
                        column: x => x.mapping_table_id,
                        principalSchema: "data",
                        principalTable: "mapping_tables",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_keyword_public_services_public_service_id",
                        column: x => x.public_service_id,
                        principalSchema: "data",
                        principalTable: "public_services",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "public_service_is_described_ats",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_described_at_id = table.Column<Guid>(type: "uuid", nullable: false),
                    public_service_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creation_type = table.Column<int>(type: "integer", nullable: true),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_public_service_is_described_ats", x => x.id);
                    table.ForeignKey(
                        name: "fk_public_service_is_described_ats_dataset_is_described_at_id",
                        column: x => x.is_described_at_id,
                        principalSchema: "data",
                        principalTable: "dataset",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_public_service_is_described_ats_public_services_public_serv~",
                        column: x => x.public_service_id,
                        principalSchema: "data",
                        principalTable: "public_services",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "public_service_relations",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    public_service_id = table.Column<Guid>(type: "uuid", nullable: false),
                    relation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creation_type = table.Column<int>(type: "integer", nullable: true),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_public_service_relations", x => x.id);
                    table.ForeignKey(
                        name: "fk_public_service_relations_public_services_public_service_id",
                        column: x => x.public_service_id,
                        principalSchema: "data",
                        principalTable: "public_services",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_public_service_relations_public_services_relation_id",
                        column: x => x.relation_id,
                        principalSchema: "data",
                        principalTable: "public_services",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "public_service_requires",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    public_service_id = table.Column<Guid>(type: "uuid", nullable: false),
                    requires_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creation_type = table.Column<int>(type: "integer", nullable: true),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_public_service_requires", x => x.id);
                    table.ForeignKey(
                        name: "fk_public_service_requires_public_services_public_service_id",
                        column: x => x.public_service_id,
                        principalSchema: "data",
                        principalTable: "public_services",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_public_service_requires_public_services_requires_id",
                        column: x => x.requires_id,
                        principalSchema: "data",
                        principalTable: "public_services",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "dcat_catalog_resource",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    resource_id = table.Column<Guid>(type: "uuid", nullable: false),
                    resource_type = table.Column<string>(type: "text", nullable: false),
                    dcat_catalog_record_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creation_type = table.Column<int>(type: "integer", nullable: true),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dcat_catalog_resource", x => x.id);
                    table.ForeignKey(
                        name: "fk_dcat_catalog_resource_dcat_catalog_record_dcat_catalog_reco~",
                        column: x => x.dcat_catalog_record_id,
                        principalSchema: "data",
                        principalTable: "dcat_catalog_record",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "dcat_catalog_theme",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    dcat_catalog_record_id = table.Column<Guid>(type: "uuid", nullable: false),
                    theme_taxonomy = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creation_type = table.Column<int>(type: "integer", nullable: true),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dcat_catalog_theme", x => x.id);
                    table.ForeignKey(
                        name: "fk_dcat_catalog_theme_dcat_catalog_record_dcat_catalog_record_~",
                        column: x => x.dcat_catalog_record_id,
                        principalSchema: "data",
                        principalTable: "dcat_catalog_record",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "check_sum",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    algorithm = table.Column<string>(type: "text", nullable: true),
                    check_sum_value = table.Column<string>(type: "text", nullable: false),
                    distribution_checksum_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creation_type = table.Column<int>(type: "integer", nullable: true),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_check_sum", x => x.id);
                    table.ForeignKey(
                        name: "fk_check_sum_distribution_distribution_checksum_id",
                        column: x => x.distribution_checksum_id,
                        principalSchema: "data",
                        principalTable: "distribution",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "distribution_data_service_relations",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    distribution_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_service_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creation_type = table.Column<int>(type: "integer", nullable: true),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_distribution_data_service_relations", x => x.id);
                    table.ForeignKey(
                        name: "fk_distribution_data_service_relations_data_service_data_servi~",
                        column: x => x.data_service_id,
                        principalSchema: "data",
                        principalTable: "data_service",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_distribution_data_service_relations_distribution_distributi~",
                        column: x => x.distribution_id,
                        principalSchema: "data",
                        principalTable: "distribution",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "period_of_time",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    start = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    temporal_coverage_id = table.Column<Guid>(type: "uuid", nullable: true),
                    distribution_coverage_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creation_type = table.Column<int>(type: "integer", nullable: true),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_period_of_time", x => x.id);
                    table.ForeignKey(
                        name: "fk_period_of_time_dataset_temporal_coverage_id",
                        column: x => x.temporal_coverage_id,
                        principalSchema: "data",
                        principalTable: "dataset",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_period_of_time_distribution_distribution_coverage_id",
                        column: x => x.distribution_coverage_id,
                        principalSchema: "data",
                        principalTable: "distribution",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "resource",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_service_conforms_to_id = table.Column<Guid>(type: "uuid", nullable: true),
                    data_service_documentation_id = table.Column<Guid>(type: "uuid", nullable: true),
                    data_service_endpoint_description_id = table.Column<Guid>(type: "uuid", nullable: true),
                    data_service_endpoint_url_id = table.Column<Guid>(type: "uuid", nullable: true),
                    data_service_landing_page_id = table.Column<Guid>(type: "uuid", nullable: true),
                    data_set_conforms_to_id = table.Column<Guid>(type: "uuid", nullable: true),
                    dataset_documentation_id = table.Column<Guid>(type: "uuid", nullable: true),
                    data_set_image_id = table.Column<Guid>(type: "uuid", nullable: true),
                    data_set_is_referenced_by_id = table.Column<Guid>(type: "uuid", nullable: true),
                    dataset_landing_page_id = table.Column<Guid>(type: "uuid", nullable: true),
                    data_set_relation_id = table.Column<Guid>(type: "uuid", nullable: true),
                    distribution_access_url_id = table.Column<Guid>(type: "uuid", nullable: true),
                    distribution_conforms_to_id = table.Column<Guid>(type: "uuid", nullable: true),
                    distribution_documentation_id = table.Column<Guid>(type: "uuid", nullable: true),
                    distribution_download_url_id = table.Column<Guid>(type: "uuid", nullable: true),
                    distribution_image_id = table.Column<Guid>(type: "uuid", nullable: true),
                    iop_concept_conforms_to_id = table.Column<Guid>(type: "uuid", nullable: true),
                    mapping_table_conforms_to_id = table.Column<Guid>(type: "uuid", nullable: true),
                    href = table.Column<string>(type: "text", nullable: false),
                    label_de = table.Column<string>(type: "text", nullable: true),
                    label_en = table.Column<string>(type: "text", nullable: true),
                    label_fr = table.Column<string>(type: "text", nullable: true),
                    label_it = table.Column<string>(type: "text", nullable: true),
                    label_rm = table.Column<string>(type: "text", nullable: true),
                    qualified_relation_id = table.Column<Guid>(type: "uuid", nullable: true),
                    agent_image_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creation_type = table.Column<int>(type: "integer", nullable: true),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_resource", x => x.id);
                    table.ForeignKey(
                        name: "fk_resource_agent_agent_image_id",
                        column: x => x.agent_image_id,
                        principalSchema: "data",
                        principalTable: "agent",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_resource_data_service_data_service_conforms_to_id",
                        column: x => x.data_service_conforms_to_id,
                        principalSchema: "data",
                        principalTable: "data_service",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_resource_data_service_data_service_documentation_id",
                        column: x => x.data_service_documentation_id,
                        principalSchema: "data",
                        principalTable: "data_service",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_resource_data_service_data_service_endpoint_description_id",
                        column: x => x.data_service_endpoint_description_id,
                        principalSchema: "data",
                        principalTable: "data_service",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_resource_data_service_data_service_endpoint_url_id",
                        column: x => x.data_service_endpoint_url_id,
                        principalSchema: "data",
                        principalTable: "data_service",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_resource_data_service_data_service_landing_page_id",
                        column: x => x.data_service_landing_page_id,
                        principalSchema: "data",
                        principalTable: "data_service",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_resource_dataset_data_set_conforms_to_id",
                        column: x => x.data_set_conforms_to_id,
                        principalSchema: "data",
                        principalTable: "dataset",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_resource_dataset_data_set_image_id",
                        column: x => x.data_set_image_id,
                        principalSchema: "data",
                        principalTable: "dataset",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_resource_dataset_data_set_is_referenced_by_id",
                        column: x => x.data_set_is_referenced_by_id,
                        principalSchema: "data",
                        principalTable: "dataset",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_resource_dataset_data_set_relation_id",
                        column: x => x.data_set_relation_id,
                        principalSchema: "data",
                        principalTable: "dataset",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_resource_dataset_dataset_documentation_id",
                        column: x => x.dataset_documentation_id,
                        principalSchema: "data",
                        principalTable: "dataset",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_resource_dataset_dataset_landing_page_id",
                        column: x => x.dataset_landing_page_id,
                        principalSchema: "data",
                        principalTable: "dataset",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_resource_distribution_distribution_access_url_id",
                        column: x => x.distribution_access_url_id,
                        principalSchema: "data",
                        principalTable: "distribution",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_resource_distribution_distribution_conforms_to_id",
                        column: x => x.distribution_conforms_to_id,
                        principalSchema: "data",
                        principalTable: "distribution",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_resource_distribution_distribution_documentation_id",
                        column: x => x.distribution_documentation_id,
                        principalSchema: "data",
                        principalTable: "distribution",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_resource_distribution_distribution_download_url_id",
                        column: x => x.distribution_download_url_id,
                        principalSchema: "data",
                        principalTable: "distribution",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_resource_distribution_distribution_image_id",
                        column: x => x.distribution_image_id,
                        principalSchema: "data",
                        principalTable: "distribution",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_resource_iop_concepts_iop_concept_conforms_to_id",
                        column: x => x.iop_concept_conforms_to_id,
                        principalSchema: "data",
                        principalTable: "iop_concepts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_resource_mapping_tables_mapping_table_conforms_to_id",
                        column: x => x.mapping_table_conforms_to_id,
                        principalSchema: "data",
                        principalTable: "mapping_tables",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_resource_qualified_relation_qualified_relation_id",
                        column: x => x.qualified_relation_id,
                        principalSchema: "data",
                        principalTable: "qualified_relation",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "annotations",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code_list_entry_id = table.Column<Guid>(type: "uuid", nullable: false),
                    identifier = table.Column<string>(type: "text", nullable: true),
                    position = table.Column<int>(type: "integer", nullable: false),
                    text_de = table.Column<string>(type: "text", nullable: true),
                    text_en = table.Column<string>(type: "text", nullable: true),
                    text_fr = table.Column<string>(type: "text", nullable: true),
                    text_it = table.Column<string>(type: "text", nullable: true),
                    text_rm = table.Column<string>(type: "text", nullable: true),
                    title = table.Column<string>(type: "text", nullable: true),
                    type = table.Column<string>(type: "text", nullable: false),
                    uri = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creation_type = table.Column<int>(type: "integer", nullable: true),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_annotations", x => x.id);
                    table.ForeignKey(
                        name: "fk_annotations_code_list_entries_code_list_entry_id",
                        column: x => x.code_list_entry_id,
                        principalSchema: "data",
                        principalTable: "code_list_entries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "channel_owned_bys",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    channel_id = table.Column<Guid>(type: "uuid", nullable: false),
                    owned_by_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    creation_type = table.Column<int>(type: "integer", nullable: true),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_channel_owned_bys", x => x.id);
                    table.ForeignKey(
                        name: "fk_channel_owned_bys_agent_owned_by_id",
                        column: x => x.owned_by_id,
                        principalSchema: "data",
                        principalTable: "agent",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_channel_owned_bys_channels_channel_id",
                        column: x => x.channel_id,
                        principalSchema: "data",
                        principalTable: "channels",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "data",
                table: "iop_persons",
                columns: new[] { "id", "created_at", "creation_type", "email", "family_name", "first_login_date", "given_name", "last_login_date", "modified_at" },
                values: new object[] { new Guid("56c59f4f-02a0-46b8-b691-2328b6f02054"), new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "max.muster@example.org", "Muster", new DateOnly(1, 1, 1), "Max", new DateOnly(1, 1, 1), null });

            migrationBuilder.CreateIndex(
                name: "ix_agent_identifier",
                schema: "data",
                table: "agent",
                column: "identifier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_agent_sub_agent_relation_agent_id_sub_agent_id",
                schema: "data",
                table: "agent_sub_agent_relation",
                columns: new[] { "agent_id", "sub_agent_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_agent_sub_agent_relation_sub_agent_id",
                schema: "data",
                table: "agent_sub_agent_relation",
                column: "sub_agent_id");

            migrationBuilder.CreateIndex(
                name: "ix_annotations_code_list_entry_id",
                schema: "data",
                table: "annotations",
                column: "code_list_entry_id");

            migrationBuilder.CreateIndex(
                name: "ix_channel_owned_bys_channel_id_owned_by_id",
                schema: "data",
                table: "channel_owned_bys",
                columns: new[] { "channel_id", "owned_by_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_channel_owned_bys_owned_by_id",
                schema: "data",
                table: "channel_owned_bys",
                column: "owned_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_channels_public_service_id",
                schema: "data",
                table: "channels",
                column: "public_service_id");

            migrationBuilder.CreateIndex(
                name: "ix_check_sum_distribution_checksum_id",
                schema: "data",
                table: "check_sum",
                column: "distribution_checksum_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_code_list_entries_iop_concept_id_code",
                schema: "data",
                table: "code_list_entries",
                columns: new[] { "iop_concept_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_code_list_entries_parent_code_list_entry_id",
                schema: "data",
                table: "code_list_entries",
                column: "parent_code_list_entry_id");

            migrationBuilder.CreateIndex(
                name: "ix_data_service_previous_version_id",
                schema: "data",
                table: "data_service",
                column: "previous_version_id");

            migrationBuilder.CreateIndex(
                name: "ix_data_service_publisher_id",
                schema: "data",
                table: "data_service",
                column: "publisher_id");

            migrationBuilder.CreateIndex(
                name: "ix_data_service_responsible_deputy_id",
                schema: "data",
                table: "data_service",
                column: "responsible_deputy_id");

            migrationBuilder.CreateIndex(
                name: "ix_data_service_responsible_person_id",
                schema: "data",
                table: "data_service",
                column: "responsible_person_id");

            migrationBuilder.CreateIndex(
                name: "ix_data_service_dataset_data_service_id_dataset_id",
                schema: "data",
                table: "data_service_dataset",
                columns: new[] { "data_service_id", "dataset_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_data_service_dataset_dataset_id",
                schema: "data",
                table: "data_service_dataset",
                column: "dataset_id");

            migrationBuilder.CreateIndex(
                name: "ix_dataset_previous_version_id",
                schema: "data",
                table: "dataset",
                column: "previous_version_id");

            migrationBuilder.CreateIndex(
                name: "ix_dataset_publisher_id",
                schema: "data",
                table: "dataset",
                column: "publisher_id");

            migrationBuilder.CreateIndex(
                name: "ix_dataset_responsible_deputy_id",
                schema: "data",
                table: "dataset",
                column: "responsible_deputy_id");

            migrationBuilder.CreateIndex(
                name: "ix_dataset_responsible_person_id",
                schema: "data",
                table: "dataset",
                column: "responsible_person_id");

            migrationBuilder.CreateIndex(
                name: "ix_dataset_quality_answer_option_dataset_quality_question_id",
                schema: "data",
                table: "dataset_quality_answer_option",
                column: "dataset_quality_question_id");

            migrationBuilder.CreateIndex(
                name: "ix_dataset_quality_information_answer_id",
                schema: "data",
                table: "dataset_quality_information",
                column: "answer_id");

            migrationBuilder.CreateIndex(
                name: "ix_dataset_quality_information_dataset_id",
                schema: "data",
                table: "dataset_quality_information",
                column: "dataset_id");

            migrationBuilder.CreateIndex(
                name: "ix_dataset_quality_information_question_id",
                schema: "data",
                table: "dataset_quality_information",
                column: "question_id");

            migrationBuilder.CreateIndex(
                name: "ix_dataset_quality_information_link_dataset_id",
                schema: "data",
                table: "dataset_quality_information_link",
                column: "dataset_id");

            migrationBuilder.CreateIndex(
                name: "ix_dcat_catalog_publisher_id",
                schema: "data",
                table: "dcat_catalog",
                column: "publisher_id");

            migrationBuilder.CreateIndex(
                name: "ix_dcat_catalog_record_dcat_catalog_id",
                schema: "data",
                table: "dcat_catalog_record",
                column: "dcat_catalog_id");

            migrationBuilder.CreateIndex(
                name: "ix_dcat_catalog_resource_dcat_catalog_record_id",
                schema: "data",
                table: "dcat_catalog_resource",
                column: "dcat_catalog_record_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_dcat_catalog_theme_dcat_catalog_record_id",
                schema: "data",
                table: "dcat_catalog_theme",
                column: "dcat_catalog_record_id");

            migrationBuilder.CreateIndex(
                name: "ix_distribution_dataset_id",
                schema: "data",
                table: "distribution",
                column: "dataset_id");

            migrationBuilder.CreateIndex(
                name: "ix_distribution_data_service_relations_data_service_id",
                schema: "data",
                table: "distribution_data_service_relations",
                column: "data_service_id");

            migrationBuilder.CreateIndex(
                name: "ix_distribution_data_service_relations_distribution_id",
                schema: "data",
                table: "distribution_data_service_relations",
                column: "distribution_id");

            migrationBuilder.CreateIndex(
                name: "ix_iop_concepts_publisher_id",
                schema: "data",
                table: "iop_concepts",
                column: "publisher_id");

            migrationBuilder.CreateIndex(
                name: "ix_iop_concepts_responsible_deputy_id",
                schema: "data",
                table: "iop_concepts",
                column: "responsible_deputy_id");

            migrationBuilder.CreateIndex(
                name: "ix_iop_concepts_responsible_person_id",
                schema: "data",
                table: "iop_concepts",
                column: "responsible_person_id");

            migrationBuilder.CreateIndex(
                name: "ix_iop_persons_email",
                schema: "data",
                table: "iop_persons",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_keyword_data_service_id",
                schema: "data",
                table: "keyword",
                column: "data_service_id");

            migrationBuilder.CreateIndex(
                name: "ix_keyword_dataset_id",
                schema: "data",
                table: "keyword",
                column: "dataset_id");

            migrationBuilder.CreateIndex(
                name: "ix_keyword_iop_concept_id",
                schema: "data",
                table: "keyword",
                column: "iop_concept_id");

            migrationBuilder.CreateIndex(
                name: "ix_keyword_mapping_table_id",
                schema: "data",
                table: "keyword",
                column: "mapping_table_id");

            migrationBuilder.CreateIndex(
                name: "ix_keyword_public_service_id",
                schema: "data",
                table: "keyword",
                column: "public_service_id");

            migrationBuilder.CreateIndex(
                name: "ix_mapping_relations_mapping_table_id_source_code_uri_target_c~",
                schema: "data",
                table: "mapping_relations",
                columns: new[] { "mapping_table_id", "source_code_uri", "target_code_uri" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_mapping_tables_publisher_id",
                schema: "data",
                table: "mapping_tables",
                column: "publisher_id");

            migrationBuilder.CreateIndex(
                name: "ix_mapping_tables_responsible_deputy_id",
                schema: "data",
                table: "mapping_tables",
                column: "responsible_deputy_id");

            migrationBuilder.CreateIndex(
                name: "ix_mapping_tables_responsible_person_id",
                schema: "data",
                table: "mapping_tables",
                column: "responsible_person_id");

            migrationBuilder.CreateIndex(
                name: "ix_period_of_time_distribution_coverage_id",
                schema: "data",
                table: "period_of_time",
                column: "distribution_coverage_id");

            migrationBuilder.CreateIndex(
                name: "ix_period_of_time_temporal_coverage_id",
                schema: "data",
                table: "period_of_time",
                column: "temporal_coverage_id");

            migrationBuilder.CreateIndex(
                name: "ix_public_service_is_described_ats_is_described_at_id",
                schema: "data",
                table: "public_service_is_described_ats",
                column: "is_described_at_id");

            migrationBuilder.CreateIndex(
                name: "ix_public_service_is_described_ats_public_service_id_is_descri~",
                schema: "data",
                table: "public_service_is_described_ats",
                columns: new[] { "public_service_id", "is_described_at_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_public_service_relations_public_service_id_relation_id",
                schema: "data",
                table: "public_service_relations",
                columns: new[] { "public_service_id", "relation_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_public_service_relations_relation_id",
                schema: "data",
                table: "public_service_relations",
                column: "relation_id");

            migrationBuilder.CreateIndex(
                name: "ix_public_service_requires_public_service_id_requires_id",
                schema: "data",
                table: "public_service_requires",
                columns: new[] { "public_service_id", "requires_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_public_service_requires_requires_id",
                schema: "data",
                table: "public_service_requires",
                column: "requires_id");

            migrationBuilder.CreateIndex(
                name: "ix_public_services_publisher_id",
                schema: "data",
                table: "public_services",
                column: "publisher_id");

            migrationBuilder.CreateIndex(
                name: "ix_public_services_responsible_deputy_id",
                schema: "data",
                table: "public_services",
                column: "responsible_deputy_id");

            migrationBuilder.CreateIndex(
                name: "ix_public_services_responsible_person_id",
                schema: "data",
                table: "public_services",
                column: "responsible_person_id");

            migrationBuilder.CreateIndex(
                name: "ix_qualified_attribution_agent_id",
                schema: "data",
                table: "qualified_attribution",
                column: "agent_id");

            migrationBuilder.CreateIndex(
                name: "ix_qualified_attribution_dataset_id",
                schema: "data",
                table: "qualified_attribution",
                column: "dataset_id");

            migrationBuilder.CreateIndex(
                name: "ix_qualified_relation_dataset_id",
                schema: "data",
                table: "qualified_relation",
                column: "dataset_id");

            migrationBuilder.CreateIndex(
                name: "ix_resource_agent_image_id",
                schema: "data",
                table: "resource",
                column: "agent_image_id");

            migrationBuilder.CreateIndex(
                name: "ix_resource_data_service_conforms_to_id",
                schema: "data",
                table: "resource",
                column: "data_service_conforms_to_id");

            migrationBuilder.CreateIndex(
                name: "ix_resource_data_service_documentation_id",
                schema: "data",
                table: "resource",
                column: "data_service_documentation_id");

            migrationBuilder.CreateIndex(
                name: "ix_resource_data_service_endpoint_description_id",
                schema: "data",
                table: "resource",
                column: "data_service_endpoint_description_id");

            migrationBuilder.CreateIndex(
                name: "ix_resource_data_service_endpoint_url_id",
                schema: "data",
                table: "resource",
                column: "data_service_endpoint_url_id");

            migrationBuilder.CreateIndex(
                name: "ix_resource_data_service_landing_page_id",
                schema: "data",
                table: "resource",
                column: "data_service_landing_page_id");

            migrationBuilder.CreateIndex(
                name: "ix_resource_data_set_conforms_to_id",
                schema: "data",
                table: "resource",
                column: "data_set_conforms_to_id");

            migrationBuilder.CreateIndex(
                name: "ix_resource_data_set_image_id",
                schema: "data",
                table: "resource",
                column: "data_set_image_id");

            migrationBuilder.CreateIndex(
                name: "ix_resource_data_set_is_referenced_by_id",
                schema: "data",
                table: "resource",
                column: "data_set_is_referenced_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_resource_data_set_relation_id",
                schema: "data",
                table: "resource",
                column: "data_set_relation_id");

            migrationBuilder.CreateIndex(
                name: "ix_resource_dataset_documentation_id",
                schema: "data",
                table: "resource",
                column: "dataset_documentation_id");

            migrationBuilder.CreateIndex(
                name: "ix_resource_dataset_landing_page_id",
                schema: "data",
                table: "resource",
                column: "dataset_landing_page_id");

            migrationBuilder.CreateIndex(
                name: "ix_resource_distribution_access_url_id",
                schema: "data",
                table: "resource",
                column: "distribution_access_url_id");

            migrationBuilder.CreateIndex(
                name: "ix_resource_distribution_conforms_to_id",
                schema: "data",
                table: "resource",
                column: "distribution_conforms_to_id");

            migrationBuilder.CreateIndex(
                name: "ix_resource_distribution_documentation_id",
                schema: "data",
                table: "resource",
                column: "distribution_documentation_id");

            migrationBuilder.CreateIndex(
                name: "ix_resource_distribution_download_url_id",
                schema: "data",
                table: "resource",
                column: "distribution_download_url_id");

            migrationBuilder.CreateIndex(
                name: "ix_resource_distribution_image_id",
                schema: "data",
                table: "resource",
                column: "distribution_image_id");

            migrationBuilder.CreateIndex(
                name: "ix_resource_iop_concept_conforms_to_id",
                schema: "data",
                table: "resource",
                column: "iop_concept_conforms_to_id");

            migrationBuilder.CreateIndex(
                name: "ix_resource_mapping_table_conforms_to_id",
                schema: "data",
                table: "resource",
                column: "mapping_table_conforms_to_id");

            migrationBuilder.CreateIndex(
                name: "ix_resource_qualified_relation_id",
                schema: "data",
                table: "resource",
                column: "qualified_relation_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_vcard_agent_contact_point_id",
                schema: "data",
                table: "vcard",
                column: "agent_contact_point_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_vcard_data_service_contact_point_id",
                schema: "data",
                table: "vcard",
                column: "data_service_contact_point_id");

            migrationBuilder.CreateIndex(
                name: "ix_vcard_dataset_contact_point_id",
                schema: "data",
                table: "vcard",
                column: "dataset_contact_point_id");

            migrationBuilder.CreateIndex(
                name: "ix_vocabulary_config_vocabulary_identifier",
                schema: "data",
                table: "vocabulary_config",
                column: "vocabulary_identifier",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "agent_sub_agent_relation",
                schema: "data");

            migrationBuilder.DropTable(
                name: "annotations",
                schema: "data");

            migrationBuilder.DropTable(
                name: "channel_owned_bys",
                schema: "data");

            migrationBuilder.DropTable(
                name: "check_sum",
                schema: "data");

            migrationBuilder.DropTable(
                name: "data_service_dataset",
                schema: "data");

            migrationBuilder.DropTable(
                name: "dataset_quality_information",
                schema: "data");

            migrationBuilder.DropTable(
                name: "dataset_quality_information_link",
                schema: "data");

            migrationBuilder.DropTable(
                name: "dcat_catalog_resource",
                schema: "data");

            migrationBuilder.DropTable(
                name: "dcat_catalog_theme",
                schema: "data");

            migrationBuilder.DropTable(
                name: "distribution_data_service_relations",
                schema: "data");

            migrationBuilder.DropTable(
                name: "keyword",
                schema: "data");

            migrationBuilder.DropTable(
                name: "mapping_relations",
                schema: "data");

            migrationBuilder.DropTable(
                name: "period_of_time",
                schema: "data");

            migrationBuilder.DropTable(
                name: "public_service_is_described_ats",
                schema: "data");

            migrationBuilder.DropTable(
                name: "public_service_relations",
                schema: "data");

            migrationBuilder.DropTable(
                name: "public_service_requires",
                schema: "data");

            migrationBuilder.DropTable(
                name: "qualified_attribution",
                schema: "data");

            migrationBuilder.DropTable(
                name: "resource",
                schema: "data");

            migrationBuilder.DropTable(
                name: "vcard",
                schema: "data");

            migrationBuilder.DropTable(
                name: "vocabulary_config",
                schema: "data");

            migrationBuilder.DropTable(
                name: "code_list_entries",
                schema: "data");

            migrationBuilder.DropTable(
                name: "channels",
                schema: "data");

            migrationBuilder.DropTable(
                name: "dataset_quality_answer_option",
                schema: "data");

            migrationBuilder.DropTable(
                name: "dcat_catalog_record",
                schema: "data");

            migrationBuilder.DropTable(
                name: "distribution",
                schema: "data");

            migrationBuilder.DropTable(
                name: "mapping_tables",
                schema: "data");

            migrationBuilder.DropTable(
                name: "qualified_relation",
                schema: "data");

            migrationBuilder.DropTable(
                name: "data_service",
                schema: "data");

            migrationBuilder.DropTable(
                name: "iop_concepts",
                schema: "data");

            migrationBuilder.DropTable(
                name: "public_services",
                schema: "data");

            migrationBuilder.DropTable(
                name: "dataset_quality_question",
                schema: "data");

            migrationBuilder.DropTable(
                name: "dcat_catalog",
                schema: "data");

            migrationBuilder.DropTable(
                name: "dataset",
                schema: "data");

            migrationBuilder.DropTable(
                name: "agent",
                schema: "data");

            migrationBuilder.DropTable(
                name: "iop_persons",
                schema: "data");
        }
    }
}
