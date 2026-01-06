const express = require('express');
const app = express();
const port = 3001;

app.use(express.json());

app.get('/api/users/phone-exists/:phoneNumber', (req, res) => {
  res.status(200).json({ exists: false });
});

app.post('/api/users', (req, res) => {
  const { userId } = req.body;
  res.status(201).json({ id: "mock-profile-id-" + userId, userId });
});

app.listen(port, () => {
  console.log(`user-service-mock (Auth) listening on port ${port}`);
});



