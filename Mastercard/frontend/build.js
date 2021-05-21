var liveServer = require("live-server");

 var params = {
     port: 8181,
     host: "localhost",
     open: true,
     file: "index.html", 
     wait: 1000,
     logLevel: 2,  
    proxy: [['/api','http://www.abc.com/api/']]
 };
 liveServer.start(params);