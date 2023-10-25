const express = require('express');
const Redis = require('ioredis');
const app = express();
const port = 3000;

// Create a Redis client to connect to the Redis server (Docker image)
const redis = new Redis({
    host: 'redis_cache', // This should match the Docker container's hostname
    port: 6379,   // Redis default port
});

app.get('/api/data', async (req, res) => {
    const { cacheKey } = req.body;

    // Try to retrieve data from the cache
    const cachedData = await redis.get(cacheKey);

    if (cachedData) {
        // Data found in the cache, return it
        res.json({ source: 'cache', data: JSON.parse(cachedData), message:'cache'});
    } else {
        // Data not found in the cache, simulate a time-consuming operation
        const data = { message: 'no cached data' };

        // Store data in the cache with a 30-second expiration
        await redis.set(cacheKey, JSON.stringify(data), 'EX', 30);

        res.json({ source: 'api', data });
    }
});

app.post('/api/data', express.json(), async (req, res) => {
    const { cacheKey, data } = req.body;

    if (!cacheKey || !data) {
        return res.status(400).json({ error: 'Cache key and data are required.' });
    }

    // Store the provided data in the cache with a 30-second expiration
    await redis.set(cacheKey, JSON.stringify(data), 'EX', 10);

    res.json({ message: 'Data has been cached successfully.' });
});

app.listen(port, () => {
    console.log(`Server is running on port ${port}`);
});
