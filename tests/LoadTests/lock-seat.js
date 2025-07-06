import http from 'k6/http';
import { check } from 'k6';

export const options = {
  vus: 10, // Virtual users (adjust as needed)
  duration: '1s', // Time for all VUs to run
};

const baseUrl = __ENV.BASE_URL;
if (!baseUrl) throw "Base URL is required to be set.";
const seatCount = 100; // Total available seats

export default function () {
  // Pick a seat at random to simulate fan choice
  const seatNumber = Math.floor(Math.random() * seatCount) + 1;

  const payload = JSON.stringify({ seatNumber });
  const headers = { 'Content-Type': 'application/json' };

  const res = http.post(`${baseUrl}/api/lock-seat`, payload, { headers });

  check(res, {
    'is status 200 or 409': r => r.status === 200 || r.status === 409,
    'is not 404':           r => r.status !== 404,
    'is not 403':           r => r.status !== 403,
  });
}
