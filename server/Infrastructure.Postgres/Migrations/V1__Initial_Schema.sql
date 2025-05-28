CREATE SCHEMA IF NOT EXISTS cleanair;

CREATE TABLE cleanair.devicelog (
                                          id text NOT NULL,
                                          deviceid text NOT NULL,
                                          value numeric NOT NULL,
                                          unit text NOT NULL,
                                          timestamp timestamp with time zone NOT NULL,
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