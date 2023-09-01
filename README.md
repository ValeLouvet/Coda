# Coda
## Assigment

### CREATE A SIMPLE API
Create a simple APPLICATION API with one endpoint which accepts an HTTP POST with any
JSON payload. The API will respond with a success response containing an exact copy of the
JSON request it received. The request and response structure can be any valid JSON, but
below is a good example to follow.
For example if you post:
{"game":"Mobile Legends", "gamerID":"GYUTDTE", "points":20}
Your API should respond with an HTTP 200 success code with a response payload of:
{"game":"Mobile Legends", "gamerID":"GYUTDTE", "points":20}
You should be able to run multiple instances of this API (for example on different ports) - for
your demo you should have at least 3 instances.

### CREATE A ROUTING API
Create an ROUND ROBIN API which will receive HTTP POSTS and send them to an instance of
your application API. The round robin API will receive the response from the application API
and send it back to the client.
You should be able to configure the round robin API with a list of application API instances e.g.
if you run 3 instances of the application api then the round robin API will need the addresses of
these instances.
When the round robin API receives a request it should choose which application API instance to
send the request to on a ‘round robin’ basis. Therefore if you have 3 instances of the
application api then the first request goes to instance 1, the second to instance 2, the third to
instance 3 etc

### Considerations
- How would my round robin API handle it if one of the application APIs goes down?
- How would my round robin API handle it if one of the application APIs starts to go
slowly?
- How would I test this application?

## Docker
### SIMPLE API
cd {work-directory}\ApplicationApi
docker build -t application-api .
docker run -d -p 8080:80 application-api
docker run -d -p 8081:80 application-api
docker run -d -p 8082:80 application-api
