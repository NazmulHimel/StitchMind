import http from 'k6/http';
import { check, sleep } from 'k6';
import { Counter, Rate, Trend } from 'k6/metrics';

// Custom Metrics for StitchMind Floor Barcode Scan Benchmark (UAT-02 / AC-05)
export const ScanDuration = new Trend('scan_duration_ms', true);
export const Sub300msSuccessRate = new Rate('sub_300ms_rate');
export const TotalScansCounter = new Counter('total_scans_processed');

export const options = {
  scenarios: {
    // 1. Warm-up phase
    warmup: {
      executor: 'ramping-vus',
      startVUs: 10,
      stages: [
        { duration: '30s', target: 50 },
        { duration: '30s', target: 100 },
      ],
      gracefulRampDown: '10s',
      tags: { phase: 'warmup' },
    },
    // 2. High-Concurrency Production Peak (2,000 concurrent virtual floor operators)
    peak_floor_burst: {
      executor: 'ramping-arrival-rate',
      startTime: '1m10s',
      startRate: 200,
      timeUnit: '1s',
      preAllocatedVUs: 200,
      maxVUs: 1000,
      stages: [
        { duration: '1m', target: 1000 },
        { duration: '2m', target: 2000 }, // 2,000 scans per second burst
        { duration: '1m', target: 500 },
      ],
      tags: { phase: 'peak_burst' },
    },
  },
  thresholds: {
    // AC-05: Floor barcode scans must execute in < 300ms
    'scan_duration_ms': ['p(90)<200', 'p(95)<300', 'p(99)<450'],
    'sub_300ms_rate': ['rate>0.99'], // >99% of all scans must be sub-300ms
    'http_req_failed': ['rate<0.005'], // <0.5% failure rate
  },
};

const BASE_URL = __ENV.API_URL || 'http://localhost:5000';

export default function () {
  const lineId = Math.floor(Math.random() * 24) + 1;
  const stationId = Math.floor(Math.random() * 50) + 1;
  const operatorBadge = `OP-${1000 + Math.floor(Math.random() * 1200)}`;
  const bundleNumber = Math.floor(Math.random() * 100000) + 1;
  const bundleBarcode = `BND-2026-CHINO-${String(bundleNumber).padStart(6, '0')}`;

  const payload = JSON.stringify({
    bundleBarcode: bundleBarcode,
    lineNumber: `LINE-${String(lineId).padStart(2, '0')}`,
    stationNumber: `STATION-${String(stationId).padStart(2, '0')}`,
    operatorBadgeId: operatorBadge,
    operationName: 'Waistband Attachment',
    piecesCount: 24,
    scanTimestampUtc: new Date().toISOString(),
  });

  const params = {
    headers: {
      'Content-Type': 'application/json',
      'X-Device-Type': 'HandheldLaserScanner-Honeywell-Xenon',
      'X-App-Version': '1.0.0-UAT',
    },
  };

  const startTime = Date.now();
  const res = http.post(`${BASE_URL}/api/sewing/scans/bundles`, payload, params);
  const latency = Date.now() - startTime;

  ScanDuration.add(latency);
  TotalScansCounter.add(1);

  const isSub300ms = latency < 300;
  Sub300msSuccessRate.add(isSub300ms);

  check(res, {
    'status is 200 or 201': (r) => r.status === 200 || r.status === 201,
    'latency is sub-300ms (AC-05)': () => isSub300ms,
    'response has scan confirmation': (r) => r.body && r.body.includes('scan'),
  });

  // Jitter sleep 50ms - 200ms simulating human physical scan interval
  sleep(0.05 + Math.random() * 0.15);
}
