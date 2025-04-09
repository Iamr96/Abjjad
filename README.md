# 📸 Image Processing API

![.NET Core](https://img.shields.io/badge/.NET-8.0-blue)
![Swagger](https://img.shields.io/badge/Swagger-UI-green)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A production-ready API for uploading images, extracting EXIF metadata, and generating resized versions.

## 🌟 Features

- **Multi-image upload** (JPG/PNG/WebP)
- **Automatic resizing** (phone/tablet/desktop)
- **EXIF metadata extraction** (camera info, GPS, etc.)
- **Swagger UI** for easy testing

## 🚀 Quick Start

```bash
git clone https://github.com/Iamr96/Abjjad.git
cd abbjad
dotnet run



## 🔍 API Documentation

Access interactive documentation via Swagger:

**🔗 [https://localhost:5001/swagger](https://localhost:5001/swagger)**

---

## 📡 Endpoints

| Method | Endpoint                          | Description                          |
|--------|-----------------------------------|--------------------------------------|
| POST   | `/api/images`                     | Upload multiple images               |
| GET    | `/api/images/{id}/{size}`         | Get resized image (phone/tablet/desktop/custom width) |
| GET    | `/api/images/{id}/metadata`       | Get image EXIF metadata              |

---

## 🛠️ Configuration

You can customize storage and validation settings by modifying the `appsettings.json` file:

```json
{
  "Storage": {
    "RootPath": "E:\\AbbjadPics" // Change this to set a different image storage path
  }
}
