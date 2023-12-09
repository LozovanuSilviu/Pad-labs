const express = require('express');
const bodyParser = require('body-parser');

const app = express();
const port = 3002; // Change this port as needed
let requestCounts = new Map([
    [200, 0],
    [400, 0],
    [500, 0],
    [404, 0],
]);

// Middleware to parse JSON requests
app.use(bodyParser.json());

// In-memory store for registered services
const registeredServices = [];

// Endpoint for services to register themselves
app.post('/register', (req, res) => {
    const { serviceName, port } = req.body;

    if (!serviceName || !port) {
        requestCounts.set(400, requestCounts.get(400) + 1);
        return res.status(400).json({ error: 'Invalid request. serviceName and port are required.' });
    }

    const service = { serviceName, port };
    registeredServices.push(service);

    console.log(`Service ${serviceName} registered on port ${port}`);
    requestCounts.set(200, requestCounts.get(200) + 1);
    res.json({ message: 'Service registered successfully.' });
});

// Endpoint to get a list of all registered services
app.get('/services', (req, res) => {
    const servicesList = registeredServices.map(service => {
        const { serviceName, port } = service;
        return { serviceName, port };
    });
    requestCounts.set(200, requestCounts.get(200) + 1);
    res.json(servicesList);
});

app.get('/metrics', async (req, res) => {
    res.send(getMetrics(requestCounts));
});

function getMetrics(requestCounts) {
    const metricsResponse = [];
    metricsResponse.push('# HELP http_requests_total The total number of HTTP requests.');
    metricsResponse.push('# TYPE http_requests_total counter');

    for (const [code, count] of requestCounts) {
        metricsResponse.push(`http_requests_total{code="${code}"} ${count}`);
    }

    return metricsResponse.join('\n');
}

// Start the server
app.listen(port, () => {
    console.log(`Service discovery server listening at http://localhost:${port}`);
});
