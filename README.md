# Collaborative Story Builder

Collaborative Story Builder is a Unity3D-based app that lets users collaboratively build stories in real time. It uses Firebase as its backend database, Google Gemini for AI-powered features, and the MyMemory API for real-time translation functionality.

---

## 🚀 Quick Start

- **Developers:**  
  1. Download **Unity3D Editor v6000.0.36f1**.  
  2. Download and import the [Firebase Unity SDK](https://firebase.google.com/docs/unity/setup).  
  3. Set build platform to **Android** (`File > Build Settings > Android > Switch Platform`).  
  4. Open the project and you're ready to develop!

- **End Users:**  
  1. Download the **APK file**.  
  2. Install it on any **Android device**.  
  3. Launch the app and start building stories with friends!

---

## Table of Contents
- [Installation (Developers)](#installation-developers)
- [Installation (End Users)](#installation-end-users)
- [Project Structure](#project-structure)
- [Database Structure](#database-structure)
- [Technologies Used](#technologies-used)

---

## Installation (Developers)

To set up the project locally as a developer:

1. **Download Unity3D Editor** version **6000.0.36f1**.
2. **Download the Firebase SDK** for Unity:
   - [Firebase Unity SDK Setup](https://firebase.google.com/docs/unity/setup)
3. **Add the Firebase SDK** to your Unity project through the editor.
4. **Set up the default build platform** for Android devices:
   - Go to `File > Build Settings > Android > Switch Platform`.

> ⚡ The database is dynamic — no SQL file is needed! It is created automatically as users interact with the app.

---

## Installation (End Users)

To install and use the app as a regular user:

1. Download the provided **APK file**.
2. Install the APK on any **Android device**.
3. Launch the app and start creating collaborative stories!

---

## Project Structure

The main directory of the project is the `Assets` folder, organized as follows:

- **Firebase/**  
  Contains all the Firebase configuration files.

- **GeminiManager/**  
  Contains all the integration files for Google Gemini AI.

- **Scripts/**  
  Includes all the scripts necessary for the app's functionality.

- **Resources/**  
  Contains all the UI elements such as buttons, backgrounds, icons, etc.

- **Classes/**  
  Defines all the main classes used throughout the app.

- **Scenes/**  
  Stores all the Unity scenes used in the app.

- **Prefabs/**  
  Contains reusable prefab objects for easier scene building.

---

## Database Structure

Since the app uses **Firestore** (NoSQL, cloud-based database), the collections and documents are structured dynamically as follows:

### Users Collection
Each document represents a user with fields:
- `avatarURL` (string)
- `email` (string)
- `userID` (string)
- `userLevel` (int)
- `username` (string)
- `wordsTyped` (int)

### Rooms Collection
Each document represents a game room with fields:
- `creatorID` (string)
- `isGameStarted` (boolean)
- `players` (array of objects: `{userID, username}`)
- `roomId` (string)

### Stories Collection
Each document represents a collaborative story with fields:
- `storyID` (string)
- `storyText` (string)
- `usernames` (array of usernames)
- `users` (array of userIDs)
- `wordsCount` (int)

### Friends Collection
Each document represents a user's friend list:
- Document ID is the user's `userID`
- `friends` (array of userIDs of friends)

---

## Technologies Used

- **Unity3D Engine** — Game and app development
- **Firebase** — Backend database and authentication
- **Google Gemini** — AI-powered story suggestions
- **MyMemory API** — Real-time translation features
- **Android Platform** — Target deployment platform

---

# 📝 Happy Story Building!