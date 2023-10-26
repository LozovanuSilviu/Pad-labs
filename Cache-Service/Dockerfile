# Use an official Node.js runtime as a parent image
FROM node:14

# Set the working directory to /app
WORKDIR /app

# Copy the package.json and package-lock.json
COPY package*.json ./

# Install project dependencies
RUN npm install

# Copy the application source code to the container
COPY . .

# Expose the port that the Express app will run on
EXPOSE 1234

# Define environment variable for the Redis host
ENV REDIS_HOST=redis_cache

# Command to run the application
CMD ["node", "app.js"]
