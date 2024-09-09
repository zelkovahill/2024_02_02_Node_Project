// HTTP 모듈 로딩
let http = require("http");

// HTTP 서버를 Listen 상태로 8000포트 사용하여 만듬.
http.createServer(function (require, response) {

    // response HTTP 타입 해더를 정의
    response.writeHead(200, { 'Content-Type': 'text/plain' });

    response.end('Hello world');

}).listen(8000);    // 8000번 포트를 사용

console.log("Server running at http://127.0.0.1:8000");