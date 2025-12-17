RT messaging backend.

`POST /api/auth/register`
Registers a new user account.

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `username` | string | Min 3 chars. Must be unique. |
| `password` | string | Min 6 chars. |

---

`POST /api/auth/login`
Authenticates a user and returns an acesss token and a refresh token.

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `username` | string | Required. |
| `password` | string | Required. |

---

`POST /api/auth/refresh`
Rotates the refresh token and generates a new access token.

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `accessToken` | string | The expired token. |
| `refreshToken` | string | The valid refresh token. |

`POST /api/messages`
Sends a message to another user.

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `receiverUsername` | string | Target user's login. |
| `content` | string | Max 2000 chars. |

---

`GET /api/messages/{username}`
Retrieves the chat history between the current user and the target user.

| Parameter | Type | Description |
| :--- | :--- | :--- |
| `username` | string | The username of the chat partner. |
