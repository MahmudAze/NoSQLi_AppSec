import { useState } from 'react'
import './App.css'

function App() {
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [isLoggedIn, setIsLoggedIn] = useState(false)
  const [error, setError] = useState('')

  const handleLogin = async (e) => {
    e.preventDefault()
    setError('')

    try {
      const response = await fetch('http://localhost:5045/api/auth/login', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        // Frontend normal string göndərir. Hücumçu bunu arxada intercept edib JSON obyektinə çevirəcək.
        body: JSON.stringify({ username, password }),
      })

      const data = await response.json()

      if (response.ok) {
        setIsLoggedIn(true)
      } else {
        setError(data.message || 'Login failed')
      }
    } catch (err) {
      setError('Network error. Is the API running?')
    }
  }

  if (isLoggedIn) {
    return (
      <div className="dashboard">
        <h1>🚨 Admin Dashboard 🚨</h1>
        <p>Welcome! You have successfully bypassed the authentication.</p>
        <div className="secret-data">
          <h3>Top Secret Users:</h3>
          <ul>
            <li>admin - Password123!</li>
            <li>{"flag{n0sq1_1nj3ct10n_m4st3r}"}</li>
          </ul>
        </div>
        <button onClick={() => setIsLoggedIn(false)}>Logout</button>
      </div>
    )
  }

  return (
    <div className="login-container">
      <h2>System Login</h2>
      {error && <p style={{ color: 'red' }}>{error}</p>}
      <form onSubmit={handleLogin}>
        <div>
          <input
            type="text"
            placeholder="Username"
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            required
          />
        </div>
        <br />
        <div>
          <input
            type="password"
            placeholder="Password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
          />
        </div>
        <br />
        <button type="submit">Login</button>
      </form>
    </div>
  )
}

export default App