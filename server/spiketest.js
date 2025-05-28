import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
    stages: [
        { duration: '10s', target: 10 },   // ramp-up to 10 users
        { duration: '20s', target: 100 }, // sudden spike to 100 users
        { duration: '10s', target: 10 },  // ramp-down to 10 users
    ],
};

const BASE_URL = 'http://79.76.54.84/api/GetLatestMeasurement';

export default function () {
    const res = http.get(BASE_URL);

    check(res, {
        'Status is 200': (r) => r.status === 200,
        'Response time < 500ms': (r) => r.timings.duration < 500,
    });

    sleep(1);
}