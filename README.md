# StudySync

A cross-platform **student productivity app** built with .NET MAUI that helps students manage assignments, track subjects, and generate intelligent daily/weekly schedules. Features Firebase cloud sync, local SQLite storage, and smart scheduling algorithms.

[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)
[![MAUI](https://img.shields.io/badge/MAUI-Cross--Platform-blue.svg)](https://dotnet.microsoft.com/apps/maui)
[![Firebase](https://img.shields.io/badge/Firebase-Cloud%20Sync-orange.svg)](https://firebase.google.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

## ✨ Features

### Core Features
- **📊 Dashboard** — Overview of assignments, completion progress, overdue alerts, and upcoming deadlines
- **📝 Assignment Management** — Create, edit, complete, and delete assignments with priority levels (High/Medium/Low)
- **📚 Subject Management** — Add subjects with instructor info, room, class days, and time schedules
- **📅 Smart Scheduling** — Auto-generates daily and weekly schedules based on:
  - Urgent assignments (due today/tomorrow) — all scheduled
  - Non-urgent assignments — 1 required + 1 optional
  - Dynamic free time that shrinks with urgent workload
  - Subject review sessions (only if class tomorrow, skip if class today)
  - Fixed meal times (Breakfast 7:00, Lunch 12:00, Dinner 18:30)
  - Break times between sessions
- **🔔 Push Notifications** — Local notifications for assignment reminders
- **☁️ Cloud Sync** — Firebase Firestore sync with offline SQLite fallback
- **🌙 Dark/Light Theme** — Theme switching with persistence

### Smart Scheduling Logic
- **Schedule Hours**: 7:00 AM — 9:00 PM (extends to midnight if urgent assignments)
- **Overdue Display**: Shows time ranges (e.g., "07:30 - 08:30") based on estimated duration
- **Free Time**: Dynamic duration based on urgency:
  - 3+ urgent assignments → max 30 min
  - 1-2 urgent → max 1.5 hours
  - 0 urgent → fills until 17:00 (review time)
- **Review Sessions**: 45-min sessions at 17:00 for subjects with class tomorrow
- **Breaks**: 15 min after assignments, 10 min between reviews

## 🏗️ Architecture

### Tech Stack
| Layer | Technology |
|-------|------------|
| **Framework** | .NET MAUI (.NET 10) |
| **UI** | XAML with MVVM pattern |
| **Cloud Database** | Firebase Firestore |
| **Local Database** | SQLite (sqlite-net-pcl) |
| **Authentication** | Firebase Auth (Email/Password) |
| **Notifications** | Plugin.LocalNotification |
| **Firebase** | Plugin.Firebase (Auth, Firestore, Core) |

### Project Structure
```
StudySync/
├── StudySync/                          # Main MAUI application
│   ├── ViewModels/                     # MVVM ViewModels
│   │   ├── DashboardViewModel.cs
│   │   ├── AssignmentViewModel.cs
│   │   ├── SubjectsViewModel.cs
│   │   ├── ScheduleViewModel.cs       # Smart scheduling logic
│   │   └── ...
│   ├── Services/                       # Business logic services
│   │   ├── AuthService.cs             # Firebase auth
│   │   ├── AssignmentService.cs       # Assignment CRUD + sync
│   │   ├── SubjectService.cs          # Subject CRUD + sync
│   │   ├── LocalDatabaseService.cs     # SQLite operations
│   │   └── NotificationService.cs     # Push notifications
│   ├── Converters/                     # XAML value converters
│   ├── Platforms/                      # Platform-specific code
│   ├── Resources/                      # Images, fonts, styles
│   ├── Pages/                         # XAML pages
│   │   ├── DashboardPage.xaml
│   │   ├── AssignmentPage.xaml
│   │   ├── SubjectsPage.xaml
│   │   ├── SchedulePage.xaml
│   │   ├── AddAssignmentPage.xaml
│   │   ├── AddSubjectPage.xaml
│   │   ├── LoginPage.xaml
│   │   ├── RegisterPage.xaml
│   │   └── SettingsPage.xaml
│   └── MauiProgram.cs                 # DI container setup
│
├── StudySync.Shared/                   # Shared models & interfaces
│   ├── Models/
│   │   ├── Assignment.cs              # Assignment entity
│   │   ├── Subject.cs                 # Subject entity
│   │   └── User.cs                    # User entity
│   ├── Services/
│   │   ├── IAssignmentService.cs
│   │   ├── ISubjectService.cs
│   │   └── IAuthService.cs
│   └── Helpers/
│       └── Validator.cs
│
├── apk/                                 # Pre-built APK outputs
│   ├── com.alloiosis.studysync-Signed.apk
│   └── com.alloiosis.studysync.apk
│
└── .github/                            # GitHub templates
    └── copilot-instructions.md
```

## 🚀 Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later
- [Visual Studio 2022](https://visualstudio.microsoft.com/) with MAUI workload (Windows)
- OR [Visual Studio for Mac](https://visualstudio.microsoft.com/vs/mac/) / [VS Code](https://code.visualstudio.com/) with C# Dev Kit
- Android SDK (for Android builds)
- Xcode (for iOS builds, macOS only)
- Firebase project with Auth and Firestore enabled

### Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/StudySync.git
   cd StudySync
   ```

2. **Configure Firebase**
   
   For Android:
   - Download `google-services.json` from your Firebase project
   - Place it in `StudySync/Platforms/Android/`
   
   For iOS:
   - Download `GoogleService-Info.plist` from your Firebase project
   - Place it in `StudySync/Platforms/iOS/`

3. **Restore dependencies**
   ```bash
   dotnet restore StudySync.slnx
   ```

4. **Build the project**
   ```bash
   dotnet build StudySync.slnx
   ```

### Running the App

**Android (Emulator or Device):**
```bash
cd StudySync
dotnet build -t:Run -f net10.0-android
```

**iOS (macOS only):**
```bash
cd StudySync
dotnet build -t:Run -f net10.0-ios
```

**Windows:**
```bash
cd StudySync
dotnet build -t:Run -f net10.0-windows10.0.19041.0
```

```

## 🔧 Configuration

### Firebase Setup Checklist
1. Create a Firebase project at [console.firebase.google.com](https://console.firebase.google.com)
2. Enable **Email/Password** authentication
3. Enable **Cloud Firestore** database
4. Set Firestore rules (for development):
   ```
   rules_version = '2';
   service cloud.firestore {
     match /databases/{database}/documents {
       match /{document=**} {
         allow read, write: if request.auth != null;
       }
     }
   }
   ```
5. Register Android app with package name `com.companyname.studysync`
6. Download and add `google-services.json`

### Data Models

**Assignment:**
```csharp
{
  FirestoreId: string,
  UserUid: string,
  Title: string,
  Description: string,
  SubjectName: string,
  Priority: "High" | "Medium" | "Low",
  EstimatedMinutes: int,
  DueDate: DateTime,
  IsCompleted: bool
}
```

**Subject:**
```csharp
{
  FirestoreId: string,
  UserUid: string,
  Name: string,
  Instructor: string,
  Room: string,
  ClassDays: "Mon;Wed;Fri",     // Semicolon-separated
  ClassStartTime: "10:00",       // 24-hour format
  ClassEndTime: "11:30",         // 24-hour format
  Schedule: "legacy display",
  Color: "#4A90D9"
}
```

## 📋 Development Notes

### Key Design Decisions
- **Dual Storage**: Firebase for cloud sync, SQLite for offline-first experience
- **MVVM Pattern**: Clean separation between UI and business logic
- **Dependency Injection**: Services registered in `MauiProgram.cs`
- **Smart Scheduling**: Algorithm in `ScheduleViewModel.cs` prioritizes urgency
- **Notifications**: Local notifications scheduled on assignment creation

### UI/UX Guidelines
See [UI_UX_GUIDE.md](UI_UX_GUIDE.md) for detailed interface specifications including:
- Class day selection (multi-select buttons)
- Time picker implementation
- Validation rules
- Accessibility considerations

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request


## 🙏 Acknowledgments

- [.NET MAUI](https://dotnet.microsoft.com/apps/maui) — Cross-platform framework
- [Plugin.Firebase](https://github.com/TobiasBuchholz/Plugin.Firebase) — Firebase bindings for .NET MAUI
- [sqlite-net](https://github.com/praeclarum/sqlite-net) — SQLite ORM
- [Plugin.LocalNotification](https://github.com/thudugala/Plugin.LocalNotification) — Local notifications

---

**Made with ❤️ for students everywhere**

<p align="center">
  <a href="https://github.com/yourusername/StudySync">GitHub</a> •
  <a href="https://github.com/yourusername/StudySync/issues">Issues</a> •
  <a href="https://github.com/yourusername/StudySync/discussions">Discussions</a>
</p>