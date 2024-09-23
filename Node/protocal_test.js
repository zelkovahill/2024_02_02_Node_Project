const express = require("express");         // express 선언
const app = express();

let users = [
    { id: 0, data: "User 1" }                // 임시 유저 데이터
];

app.use(express.json());                    // express json 사용

app.get('/', (req, res) => {

    let result = {                          // json 형태로 선언 후 사용
        cmd: -1,
        message: 'Hello World!'
    };

    res.send(result);
})

app.post('/userdata', (req, res) => {

    const { id, data } = req.body;

    console.log(id, data);

    let result = {                          // json 형태로 선언 후 사용
        cmd: -1,
        message: ''
    };

    let user = users.find(x => x.id == id);

    if (user === undefined)              // 유저 아이디가 없음 (신규 등록)
    {
        users.push({ id, data });
        result.cmd = 1001;
        result.message = '유저 신규 등록.'
    }
    else {
        console.log(id, user.data)
        user.data = data
        result.cmd = 1002
        result.message = '데이터 갱신'
    }

    res.send(result)
})


app.listen(3000, function () {                                  // 3000포트에서 입력을 대기
    console.log('Example app listening on port 3000');
})

app.get('/userdata/list', (req, res) => {

    let result = users.sort(function (a, b) {
        return b.score - a.score
    })

    result = result.slice(0, users.length)

    res.send({
        cmd: 1101,
        message: '',
        result
    })
})