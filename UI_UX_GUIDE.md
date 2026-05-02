# UI/UX Guide: New Subject Schedule Input

## Before vs After

### BEFORE
```
Schedule Entry Field
┌─────────────────────────────────┐
│ Mon, Wed 10:00 AM               │  ← Single text field
└─────────────────────────────────┘
❌ Prone to typos ("Tue" vs "Tues")
❌ No time validation
❌ Classes always start at 8:00 AM
```

### AFTER

#### Class Days Selection
```
Class Days
┌──────┐  ┌──────┐  ┌──────┐
│ Mon  │  │ Tue  │  │ Wed  │  ← Multi-select buttons
└──────┘  └──────┘  ┌──────┐
┌──────┐  ┌──────┐  │ Wed  │  ← Selected day highlighted
│ Thu  │  │ Fri  │  └──────┘
└──────┘  ┌──────┐
          │ Sat  │
          └──────┘

✅ Fixed choices prevent typos
✅ Visual feedback (accent color on selected)
✅ Touch-friendly (large tap targets)
✅ Supports Mon-Sat only (no typos like "Sunday")
```

#### Class Time Selection
```
Class Time
┌─────────────┐     ┌─────────────┐
│ 10:00 AM    │ to  │ 11:30 AM    │  ← Native time pickers
└─────────────┘     └─────────────┘

✅ Native mobile time picker
✅ Automatic AM/PM handling
✅ No manual text entry
✅ Format validation built-in
✅ Auto-formats as hh:mm
```

---

## User Flow

### Adding a New Subject

1. **Tap "+ Add Subject"** → Navigate to AddSubjectPage

2. **Enter Basic Info**
   ```
   Subject Name: [Mathematics        ]
   Instructor:   [Dr. John Smith     ]
   Room:         [A101               ]
   ```

3. **Select Class Days** (Multi-select)
   ```
   Class Days
   [Mon] [Tue] [✓Wed] 
   [Thu] [✓Fri] [Sat]

   Selected: Wed, Fri
   ```

4. **Set Class Time** (Time Range)
   ```
   Class Time
   Start: [10:00] to End: [11:30]
   ```

5. **Choose Color**
   ```
   [🟦Blue] [🟥Red] [🟩Green] [🟨Yellow] [🟪Purple]
   ```

6. **Tap "Save Subject"**
   ```
   ✅ Subject saved to Firestore
   ✅ ClassDays: "Wed;Fri"
   ✅ ClassStartTime: "10:00"
   ✅ ClassEndTime: "11:30"
   ```

---

## Subject Display (SubjectsPage)

```
┌─────────────────────────────────────┐
│ 📚 My Subjects                      │
└─────────────────────────────────────┘

┌─────────────────────────────────────┐
│  📐 Mathematics              [✏️][🗑️]│
│  👨‍🏫 Dr. John Smith              │
│  🏢 A101                           │
│  ⏰ Wed, Fri 10:00-11:30            │
└─────────────────────────────────────┘

┌─────────────────────────────────────┐
│  📖 Chemistry                 [✏️][🗑️]│
│  👨‍🏫 Dr. Jane Doe               │
│  🏢 B205                           │
│  ⏰ Mon, Tue, Thu 14:00-15:30       │
└─────────────────────────────────────┘
```

---

## Smart Scheduler Integration

### Daily Schedule Example

```
Today: Wednesday

07:00 - 07:30  🍞 Breakfast
07:30          📍 OVERDUE: Project Report
09:00 - 10:00  📚 Review: Chemistry
10:00 - 11:30  👨‍🎓 Class: Mathematics (A101, Dr. Smith)
12:00 - 13:00  🍽️  Lunch
13:00 - 14:00  ✍️  Assignment: Calculus HW
14:00 - 14:30  ☕ Break
14:30 - 15:30  ✍️  Assignment: Physics Lab
17:00 - 17:45  📖 Review: Physics
18:30 - 19:00  🍽️  Dinner
20:30 - 21:00  🌙 Wind Down

Key Improvements:
✅ Classes use actual times (not always 8:00 AM)
✅ Accurate end times (not guessed duration)
✅ Proper buffer after classes
✅ No schedule conflicts
```

---

## Validation Rules

### What Happens When User Submits?

```
1. Subject Name
   ❌ Empty → "⚠️ Please enter the subject name."
   ✅ Provided → Continue

2. Class Days
   ❌ None selected → "⚠️ Please select at least one class day."
   ✅ 1+ selected → Continue

3. Class Times
   ❌ Start >= End → "⚠️ Start time must be before end time."
   ✅ Start < End → Continue

4. All valid → Save to Firestore
   ✅ Subject created successfully
   ✅ Navigate back to SubjectsPage
```

---

## Data Storage Format

### Firestore Document
```json
{
  "name": "Mathematics",
  "instructor": "Dr. John Smith",
  "room": "A101",
  "classDays": "Mon;Wed;Fri",
  "classStartTime": "10:00",
  "classEndTime": "11:30",
  "schedule": "Mon, Wed, Fri 10:00-11:30",
  "color": "#4A90D9",
  "userUid": "user@example.com"
}
```

### SQLite Local Database
```sql
subjects table:
- Id (INTEGER PRIMARY KEY)
- Name, Instructor, Room
- ClassDays (Mon;Wed;Fri)
- ClassStartTime (10:00)
- ClassEndTime (11:30)
- Schedule (legacy)
- Color (#4A90D9)
```

---

## Accessibility

- ✅ Large touch targets (56dp minimum for day buttons)
- ✅ High contrast: selected vs unselected states
- ✅ Native time picker with system fonts
- ✅ Clear labels for all inputs
- ✅ Error messages provide actionable guidance
- ✅ No ambiguous time formats (always 24-hour)
