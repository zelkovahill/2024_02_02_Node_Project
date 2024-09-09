let express = require('express');                               // express 모듈을 가져옴
let app = express();                                            // express를 App 이름으로 정의하고 사용

app.get('/', function (req, res) {                              // 기본 라이터에서 Hello World를 반환
    res.send('Hello World');
});

app.get('/about', function (req, res) {                              // about 라우터에서 다음을 반환
    res.send('about World');
});

app.get('/ScoreData', function (req, res) {                              // about 라우터에서 다음을 반환
    res.send('ScoreData World');
});

app.listen(3000, function () {                                  // 3000포트에서 입력을 대기
    console.log('Example app listening on port 3000');
})