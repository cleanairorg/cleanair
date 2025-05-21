CREATE TABLE weatherstation.device_threshold (
    id TEXT PRIMARY KEY,
    deviceid TEXT NOT NULL,
    metric TEXT NOT NULL,
    warn_min NUMERIC NOT NULL,
    good_min NUMERIC NOT NULL,
    good_max NUMERIC NOT NULL,
    warn_max NUMERIC NOT NULL
);