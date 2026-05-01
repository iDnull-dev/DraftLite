CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260312082655_InitialCreate') THEN
    CREATE TABLE "projectRole" (
        "Id" uuid NOT NULL,
        "Name" character varying(64) NOT NULL,
        CONSTRAINT "PK_projectRole" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260312082655_InitialCreate') THEN
    CREATE TABLE role (
        "Id" uuid NOT NULL,
        "Name" character varying(64) NOT NULL,
        CONSTRAINT "PK_role" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260312082655_InitialCreate') THEN
    CREATE TABLE users (
        "Id" uuid NOT NULL,
        "GoogleId" character varying(256),
        "Email" character varying(320) NOT NULL,
        "Pseudo" character varying(128) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (now()),
        "IsActive" boolean NOT NULL DEFAULT TRUE,
        "BanAt" timestamp with time zone,
        "BanReason" character varying(512),
        "RoleId" uuid NOT NULL,
        CONSTRAINT "PK_users" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_users_role_RoleId" FOREIGN KEY ("RoleId") REFERENCES role ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260312082655_InitialCreate') THEN
    CREATE TABLE audit_log (
        "Id" uuid NOT NULL,
        "EntityType" character varying(128) NOT NULL,
        "EntityId" uuid NOT NULL,
        "UserId" uuid,
        "Action" character varying(64) NOT NULL,
        "ChangedAt" timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT "PK_audit_log" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_audit_log_users_UserId" FOREIGN KEY ("UserId") REFERENCES users ("Id") ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260312082655_InitialCreate') THEN
    CREATE TABLE projects (
        "Id" uuid NOT NULL,
        "OwnerId" uuid NOT NULL,
        "Title" character varying(256) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (now()),
        "UpdatedAt" timestamp with time zone NOT NULL DEFAULT (now()),
        "DeletedAt" timestamp with time zone,
        CONSTRAINT "PK_projects" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_projects_users_OwnerId" FOREIGN KEY ("OwnerId") REFERENCES users ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260312082655_InitialCreate') THEN
    CREATE TABLE pages (
        "Id" uuid NOT NULL,
        "ProjectId" uuid NOT NULL,
        "Title" character varying(256) NOT NULL,
        "Blocks" jsonb NOT NULL,
        "OrderIndex" integer NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (now()),
        "UpdatedAt" timestamp with time zone NOT NULL DEFAULT (now()),
        "DeletedAt" timestamp with time zone,
        CONSTRAINT "PK_pages" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_pages_projects_ProjectId" FOREIGN KEY ("ProjectId") REFERENCES projects ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260312082655_InitialCreate') THEN
    CREATE TABLE project_collaborators (
        "ProjectId" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "RoleId" uuid NOT NULL,
        "InvitedById" uuid NOT NULL,
        CONSTRAINT "PK_project_collaborators" PRIMARY KEY ("ProjectId", "UserId"),
        CONSTRAINT "FK_project_collaborators_projectRole_RoleId" FOREIGN KEY ("RoleId") REFERENCES "projectRole" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_project_collaborators_projects_ProjectId" FOREIGN KEY ("ProjectId") REFERENCES projects ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_project_collaborators_users_InvitedById" FOREIGN KEY ("InvitedById") REFERENCES users ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_project_collaborators_users_UserId" FOREIGN KEY ("UserId") REFERENCES users ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260312082655_InitialCreate') THEN
    CREATE TABLE "projectHistory" (
        "Id" uuid NOT NULL,
        "ProjectId" uuid NOT NULL,
        "PageId" uuid NOT NULL,
        "UserId" uuid,
        "Action" character varying(64) NOT NULL,
        "BaseVersion" integer NOT NULL,
        "Version" integer NOT NULL,
        "Patch" jsonb NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT "PK_projectHistory" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_projectHistory_pages_PageId" FOREIGN KEY ("PageId") REFERENCES pages ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_projectHistory_projects_ProjectId" FOREIGN KEY ("ProjectId") REFERENCES projects ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_projectHistory_users_UserId" FOREIGN KEY ("UserId") REFERENCES users ("Id") ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260312082655_InitialCreate') THEN
    CREATE INDEX "IX_audit_log_EntityType_EntityId" ON audit_log ("EntityType", "EntityId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260312082655_InitialCreate') THEN
    CREATE INDEX "IX_audit_log_UserId" ON audit_log ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260312082655_InitialCreate') THEN
    CREATE INDEX "IX_pages_ProjectId_OrderIndex" ON pages ("ProjectId", "OrderIndex");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260312082655_InitialCreate') THEN
    CREATE INDEX "IX_project_collaborators_InvitedById" ON project_collaborators ("InvitedById");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260312082655_InitialCreate') THEN
    CREATE INDEX "IX_project_collaborators_RoleId" ON project_collaborators ("RoleId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260312082655_InitialCreate') THEN
    CREATE INDEX "IX_project_collaborators_UserId" ON project_collaborators ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260312082655_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_projectHistory_PageId_Version" ON "projectHistory" ("PageId", "Version");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260312082655_InitialCreate') THEN
    CREATE INDEX "IX_projectHistory_ProjectId" ON "projectHistory" ("ProjectId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260312082655_InitialCreate') THEN
    CREATE INDEX "IX_projectHistory_UserId" ON "projectHistory" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260312082655_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_projectRole_Name" ON "projectRole" ("Name");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260312082655_InitialCreate') THEN
    CREATE INDEX "IX_projects_OwnerId" ON projects ("OwnerId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260312082655_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_role_Name" ON role ("Name");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260312082655_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_users_Email" ON users ("Email");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260312082655_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_users_GoogleId" ON users ("GoogleId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260312082655_InitialCreate') THEN
    CREATE INDEX "IX_users_RoleId" ON users ("RoleId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260312082655_InitialCreate') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260312082655_InitialCreate', '9.0.0');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260410081614_AddUserTheme') THEN
    ALTER TABLE users ADD "Theme" character varying(16) NOT NULL DEFAULT 'light';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260410081614_AddUserTheme') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260410081614_AddUserTheme', '9.0.0');
    END IF;
END $EF$;
COMMIT;

