const express = require('express');
const Redis = require('ioredis');
const app = express();
const axios = require('axios');
const port = 1234;
let requestCounts = new Map([
    [200, 0],
    [400, 0],
    [500, 0],
    [404, 0],
]);


// Create a Redis client to connect to the Redis server (Docker image)
const redis = new Redis({
    host: 'redis_cache', // This should match the Docker container's hostname
    port: 6379,   // Redis default port
});

redis.on('connect', () => {
    console.log('Connected to Redis');
});

// Listen for the 'error' event to handle connection errors
redis.on('error', (err) => {
    console.error('Redis connection error:', err);
});

app.get('/api/data', (req, res) => {
    console.log("get cache")
    const {cacheKey} = req.query;
    console.log(cacheKey+"key")


    if (!cacheKey) {
         res.status(400).json({ error: 'Cache key and data is required.' });
    }
    res.setHeader('Cache-Control', 'no-cache, no-store, must-revalidate');
    redis.get(cacheKey)
        .then((cachedData) => {
            if (cachedData) {
                try {
                      res.json({ source: 'cache', data: cachedData, message: 'succeeded' });
                } catch (error) {
                    console.error('JSON parsing error:', error);
                    res.status(500).json({ error: 'Internal Server Error' });
                }
            } else {
                // Handle the case when no data is found in the cache
                res.status(404).json({ message: 'failed' });
            }
        })
        .catch((error) => {
            console.error('Redis error2:', error);
            requestCounts.set(500, requestCounts.get(500) + 1);
            res.status(500).json({ error: 'Internal Server Error' });
        });

});

app.post('/api/data', express.json(), (req, res) => {
    console.log("add cache")

    const {cacheKey, data} = req.body;
    console.log(cacheKey,data);
    if (!cacheKey || !data) {
        requestCounts.set(400, requestCounts.get(400) + 1);
         return res.status(400).json({ error: 'Cache key and data are required.' });
    }

    redis.set(cacheKey, data, 'EX', 3600).then(() => {
        console.log("exited sucess")
        return res.json({ message: 'Data has been cached successfully.' });
    }).catch((error) => {
        console.error('Redis error3:', error);
        return res.status(500).json({ error: 'Internal Server Error' });
    });
});

app.post('/api/clear-cache', (req, res) => {
    console.log("cache service clear")
    redis.flushall();
    res.json({ message: 'Cache deleted successfully' });
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

app.listen(port, async () => {
    const serviceName = process.env.INSTANCE_NAME;
    const port = process.env.PORT;
    console.log(serviceName + port);

    const registerServiceModel = {
        serviceName: serviceName,
        port: port,
    };

    const url = 'http://service-discovery:3002/register';

    try {
        const response = await axios.post(url, registerServiceModel);
        console.log(response.data);
    } catch (error) {
        console.error('Error registering service:', error.message);
    }
    console.log(`Server is running on port ${port}`);
});
