INSERT INTO cleanair.device_threshold (id, metric, warn_min, good_min, good_max, warn_max)
VALUES
    (gen_random_uuid(), 'temperature', 18, 20, 23, 25),
    (gen_random_uuid(), 'humidity', 20, 30, 50, 70),
    (gen_random_uuid(), 'pressure', 1000, 1005, 1015, 1020),
    (gen_random_uuid(), 'airquality', 0, 0, 1200, 2500);
