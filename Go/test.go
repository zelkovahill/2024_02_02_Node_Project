package main

import (
	"encoding/json"
	"fmt"
	"net/http"
	"sync"
)

type User struct {
	ID   int    `json:"id"`
	Data string `json:"data"`
}

var (
	users = []User{{ID: 0, Data: "User 1"}}
	mutex = &sync.Mutex{} // 동시성을 위해 뮤텍스 사용
)

func main() {
	http.HandleFunc("/", handleRoot)                  // 기본 경로
	http.HandleFunc("/userdata", handleUserData)      // 유저 데이터 처리
	http.HandleFunc("/userdata/list", handleUserList) // 유저 리스트 처리

	fmt.Println("Server running on port 3000")
	http.ListenAndServe(":3000", nil) // 서버 시작
}

// 루트 경로 핸들러
func handleRoot(w http.ResponseWriter, r *http.Request) {
	response := map[string]interface{}{
		"cmd":     -1,
		"message": "Hello World!",
	}
	jsonResponse(w, response)
}

// 유저 데이터 처리 핸들러 (POST)
func handleUserData(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodPost {
		http.Error(w, "Invalid request method", http.StatusMethodNotAllowed)
		return
	}

	var reqBody User
	if err := json.NewDecoder(r.Body).Decode(&reqBody); err != nil {
		http.Error(w, "Invalid request body", http.StatusBadRequest)
		return
	}

	mutex.Lock() // 유저 데이터 접근 시 동시성 제어
	defer mutex.Unlock()

	result := map[string]interface{}{
		"cmd":     -1,
		"message": "",
	}

	user := findUserByID(reqBody.ID)
	if user == nil {
		users = append(users, User{ID: reqBody.ID, Data: reqBody.Data})
		result["cmd"] = 1001
		result["message"] = "유저 신규 등록."
		fmt.Printf("New user registered: ID=%d, Data=%s\n", reqBody.ID, reqBody.Data)
	} else {
		user.Data = reqBody.Data
		result["cmd"] = 1002
		result["message"] = "데이터 갱신"
		fmt.Printf("User data updated: ID=%d, New Data=%s\n", user.ID, user.Data)
	}

	jsonResponse(w, result)
}

// 유저 리스트 핸들러 (GET)
func handleUserList(w http.ResponseWriter, r *http.Request) {
	mutex.Lock()
	defer mutex.Unlock()

	// 유저 리스트를 그대로 반환
	result := map[string]interface{}{
		"cmd":     1101,
		"message": "",
		"result":  users,
	}

	fmt.Println("User list requested")
	jsonResponse(w, result)
}

// ID로 유저 찾는 함수
func findUserByID(id int) *User {
	for i := range users {
		if users[i].ID == id {
			return &users[i]
		}
	}
	return nil
}

// JSON 응답 헬퍼 함수
func jsonResponse(w http.ResponseWriter, data map[string]interface{}) {
	w.Header().Set("Content-Type", "application/json")
	json.NewEncoder(w).Encode(data)
}
