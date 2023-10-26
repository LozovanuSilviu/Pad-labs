const express = require('express');
const Redis = require('ioredis');
const app = express();
const port = 1234;

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
            console.log(cachedData+"cache")
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
            res.status(500).json({ error: 'Internal Server Error' });
        });

});

app.post('/api/data', express.json(), (req, res) => {
    console.log("add cache")

    const {cacheKey, data} = req.body;
    if (!cacheKey || !data) {
         res.status(400).json({ error: 'Cache key and data are required.' });
    }

    redis.set(cacheKey, data, 'EX', 240).then(() => {
        res.json({ message: 'Data has been cached successfully.' });
    }).catch((error) => {
        console.error('Redis error3:', error);
        res.status(500).json({ error: 'Internal Server Error' });
    });
});

app.listen(port, () => {
    console.log(`Server is running on port ${port}`);
});
