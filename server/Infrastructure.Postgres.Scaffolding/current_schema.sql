-- This schema is generated based on the current DBContext. Please check the class Seeder to see.
DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM pg_namespace WHERE nspname = 'cleanair') THEN
        CREATE SCHEMA cleanair;
    END IF;
END $EF$;


CREATE TABLE cleanair.device_threshold (
    id text NOT NULL,
    metric text NOT NULL,
    warn_min numeric NOT NULL,
    good_min numeric NOT NULL,
    good_max numeric NOT NULL,
    warn_max numeric NOT NULL,
    CONSTRAINT device_threshold_pkey PRIMARY KEY (id)
);


CREATE TABLE cleanair.devicelog (
    id text NOT NULL,
    deviceid text NOT NULL,
    unit text NOT NULL,
    timestamp timestamp with time zone NOT NULL,
    temperature numeric(4,2) NOT NULL,
    humidity numeric(4,2) NOT NULL,
    pressure numeric(6,2) NOT NULL,
    airquality integer NOT NULL,
    interval integer NOT NULL DEFAULT 15,
    CONSTRAINT devicelog_pkey PRIMARY KEY (id)
);


CREATE TABLE cleanair."user" (
    id text NOT NULL,
    email text NOT NULL,
    hash text NOT NULL,
    salt text NOT NULL,
    role text NOT NULL,
    CONSTRAINT user_pkey PRIMARY KEY (id)
);


