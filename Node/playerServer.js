const WebSocket = require('ws');

class GameServer {
    constructor(port) {
        this.wss = new WebSocket.Server({ port });
        this.clients = new Set();               // 1차 단순 클라이언트 관리
        this.players = new Map();               // 2차 플레이어 위치 관리 추가
        this.setupServerEvent();
        console.log(`게임 서버 포트 ${port}에서 시작되었습니다.`);
    }

    setupServerEvent() {
        this.wss.on(`connection`, (socket) => {
            // 1차 기본 접속 처리
            this.clients.add(socket);

            // 2차 플레이어 ID 생성 및 관리
            const playerID = this.generatePlayerId();

            this.players.set(playerID,
                {
                    socket: socket,
                    position: { x: 0, y: 0, z: 0 }
                }
            );

            console.log(`클라이언트 접속! ID : ${playerID}, 현재 접속자 : ${this.clients.size}`);

            // 접속한 플레이어에게 ID 전송 및 메세지
            const welcomeData = {
                type: `connection`,
                playerID: playerID,
                message: `서버에 연결되었습니다!`
            };

            socket.send(JSON.stringify(welcomeData));

            // 새로 추가 : 현재 접속해 있는 다른 플레이어들의 위치 정보 전송
            this.players.forEach((playerInfo, id) => {
                if (id !== playerID) {
                    socket.send(JSON.stringify({
                        type: 'playerPosition',
                        playerID: id,
                        position: playerInfo.position
                    }));
                }
            });

            socket.on('message', (message) => {
                try {
                    const data = JSON.parse(message);

                    // switch 문으로 메세지 타입 처리 추가
                    switch (data.type) {
                        case 'chat':
                            try {
                                const messageString = iconv.decode(Buffer.from(data.message), 'euc-kr');
                                console.log(`수신된 메세지 : `, messageString);
                                this.broadcast({
                                    type: 'chat',
                                    message: messageString
                                });
                            } catch (error) {
                                const messageString = iconv.decode(message, 'euc-kr');
                                console.log(`수신된 메세지 : `, messageString);
                                this.broadcast({
                                    type: 'chat',
                                    message: messageString
                                });
                            }
                            break;  // 'chat' case 종료

                        case 'playerPosiotion':
                            if (this.players.has(playerID)) {
                                this.players.get(playerID).position = data.position;    // 플레이어 위치 업데이트

                                // 다른 모든 클라이언트에게 위치 정보 전송
                                this.broadcast({
                                    type: 'playerInfo',
                                    playerId: playerId,
                                    position: data.position
                                });
                            }
                            break;
                    }
                }
                catch (error) {
                    console.log('메세지 처리 중 오류 ');
                }
            });

            socket.on(`close`, () => {
                this.clients.delete(socket);        // 클라이언트 제거

                this.players.delete(playerID);
                this.broadcast({
                    type: 'playerDisconnect',
                    playerID: playerID
                });
                console.log(`클라이언트 퇴장 ID : ${playerID}, 현재 접속자 : ${this.clients.size}`);
            });
        })
    }

    // 브로드 캐스트 함수 설정
    broadcast(data) {
        const message = JSON.stringify(data);
        this.clients.forEach(clients => {
            if (clients.readyState === WebSocket.OPEN) {
                clients.send(message);
            }
        });
    }

    generatePlayerId() {
        return `player_` + Math.random().toString(36).substr(2, 9);
    }
}

const gameServer = new GameServer(3000);