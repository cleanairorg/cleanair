
ALTER TABLE weatherstation.devicelog DROP COLUMN IF EXISTS value;

ALTER TABLE weatherstation.devicelog
    ADD COLUMN temperature numeric(4, 2) NOT NULL,
ADD COLUMN humidity numeric(4, 2) NOT NULL,
ADD COLUMN pressure numeric(6, 2) NOT NULL,
ADD COLUMN airquality integer NOT NULL;