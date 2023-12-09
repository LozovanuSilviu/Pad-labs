const express = require('express');
const bodyParser = require('body-parser');
const axios = require('axios');
const {BookEdit} = require("./Enums/BookEdit");

const app = express();
const port = 3000;
let inventoryServices = [];
let rentingServices = [];
let cacheService = []
let requestCounts = new Map([
    [200, 0],
    [400, 0],
    [500, 0],
    [404, 0],
]);

app.use(bodyParser.json());
let inventoryServiceIndex = 0;

function getNextInventoryService() {
    const service = inventoryServices[inventoryServiceIndex];
    inventoryServiceIndex = (inventoryServiceIndex + 1) % inventoryServices.length;
    return service;
}

// Define routes
app.get('/', (req, res) => {
    res.send('API Gateway is up and running!');
});

 async function circuitBreaker(url, serviceName, servicePort, method) {
     console.log("entered circuit breaker")
     let stop = Date.now() + 52000; // 52 seconds
     let errCount = 0;

     while (Date.now() < stop) {
         try {
             console.log("entered circuit breaker loop" + url)
             const response = await axios.get(url);
             if (response.status === 200) {
                 return true; // Service is healthy
             }
         } catch (error) {
             errCount++;
             if (errCount === 3) {
                 console.log(`Service "${serviceName}" at ${servicePort} is SICK. 3 errors in <= 52 seconds.`);
                 return false; // Service is sick
             }
         }
     }
     return false; // Circuit breaker timeout
 }

// Example route to forward requests to a service
app.get('/get-book-by-id/:id', async (req, res) => {
   let serviceInstance = getNextInventoryService();
   let rounds = inventoryServices.length;
   console.log(rounds)
   let current =0;
    const url =`http://${serviceInstance.serviceName}:${serviceInstance.port}/get-book-by-id/${req.params.id}`;
    while (current<rounds)
    {
        try {
            current++
            console.log(url)
            const response = await axios.get(url);
            requestCounts.set(200, requestCounts.get(200) + 1);
            res.json(response.data);
            break;
        }
        catch (error) {
            console.log("catched error")
            circuitBreaker(url,serviceInstance.serviceName,serviceInstance.port);
            requestCounts.set(500, requestCounts.get(500) + 1);
            res.status(500).json({ error: `Failed to fetch data from ${serviceInstance.serviceName}` });
        }
    }

});

app.get('/api/data', async (req, res) => {
    const cacheServiceUrl = `http://${cacheService[0].serviceName}:${cacheService[0].port}/api/data`;

    try {
        console.log(req.query);
        const response = await axios.get(cacheServiceUrl, {
            params: req.query, // Pass the query parameters from the original request
        });

        requestCounts.set(200, requestCounts.get(200) + 1);
        res.status(200).json(response.data);
    } catch (error) {
        if (error.message === "Request failed with status code 404")
        {
            console.log("catching")
            requestCounts.set(200, requestCounts.get(200) + 1);
            res.status(200).json({source: 'cache', data: null, message: 'succeeded'})
        }
        else
        {
            console.error('Error forwarding request to cache service:', error.message);
            requestCounts.set(500, requestCounts.get(500) + 1);
            res.status(500).json({ error: 'Failed to forward request to cache service' });
        }
    }
});

app.post('/api/data', async (req, res) => {
    const cacheServiceUrl = `http://${cacheService[0].serviceName}:${cacheService[0].port}/api/data`;

    try {
        console.log(req.body.cacheKey, req.body.data);
        const response = await axios.post(cacheServiceUrl, req.body, {
            headers: {
                'Content-Type': 'application/json', // Set the Content-Type header
            },
        });

        requestCounts.set(200, requestCounts.get(200) + 1);
        res.status(response.status).json(response.data);
    } catch (error) {
        console.error('Error forwarding request to cache service:', error.message);
        requestCounts.set(500, requestCounts.get(500) + 1);
        res.status(500).json({ error: 'Failed to forward request to cache service' });
    }
});


app.post('/api/clear-cache', async (req, res) => {
    console.log("cleared cache")
    const url =`http://${cacheService[0].serviceName}:${cacheService[0].port}/api/clear-cache`;
    console.log(url+"******")
    const response =await axios.post(url);
    res.send(response.data);
});


// Example route to forward requests to another service
app.put('/updateInfo/flag=:operationType/:id', async (req, res) => {
    const { operationType, id } = req.params;
    const flag = BookEdit[req.params.operationType]
    let serviceInstance = getNextInventoryService();
    const url =`http://${serviceInstance.serviceName}:${serviceInstance.port}/updateInfo/flag=${flag}/${req.params.id}`;
    let rounds = inventoryServices.length;
    let current =0;
    console.log(rounds)
    while (current<rounds)
    {
        try {
            current++
            // Replace the URL with the actual URL of your service2
            console.log(url)
            const response = await axios.put(url);
            requestCounts.set(200, requestCounts.get(200) + 1);
            res.json(response.data);
        } catch (error) {
             circuitBreaker(url,serviceInstance.serviceName,serviceInstance.port);
        }
        break;
    }
    requestCounts.set(500, requestCounts.get(500) + 1);
    res.status(500).json({ error: `Failed to fetch data from ${serviceInstance.serviceName}` });
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
    console.log(`Gateway server listening at http://localhost:${port}`);

    // Make a request to the service discovery endpoint
    axios.get("http://service-discovery:3002/services")
        .then(response => {
            const availableServices = response.data.map(service => {
                return { serviceName: service.serviceName, port: service.port };
            });

            inventoryServices = availableServices.filter(service => service.serviceName.includes('inventory'));
            rentingServices = availableServices.filter(service => service.serviceName.includes('renting'));
            cacheService = availableServices.filter(service => service.serviceName.includes('cache'))
            console.log('Available services1:', inventoryServices);
            console.log('Available services2:', rentingServices);
            console.log('Available services3:', cacheService);

        })
        .catch(error => {
            console.error('Error fetching services from service discovery:', error.message);
        });
});

