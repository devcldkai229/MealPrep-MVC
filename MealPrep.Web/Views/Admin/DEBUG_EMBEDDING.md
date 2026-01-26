# Debug Guide: Generate Embedding Button Không Hoạt Động

## Các Bước Kiểm Tra

### 1. Mở Browser Console (F12)
- Nhấn F12 để mở Developer Tools
- Chuyển sang tab "Console"
- Refresh trang Admin Dashboard
- Kiểm tra xem có log nào không:
  - `✓ Generate button found on page load`
  - `✓ Embedding form found`
  - `✓ Anti-forgery token found`

### 2. Click Button và Xem Console
- Click nút "Generate Embeddings Cho Tất Cả Món Ăn"
- Xem console có log:
  - `Generate button clicked`
  - `Sending request to: ...`
  - `Response status: ...`

### 3. Kiểm Tra Network Tab
- Mở tab "Network" trong Developer Tools
- Click button
- Tìm request đến `/Admin/GenerateAllMealEmbeddings`
- Xem:
  - Status code (200 = OK, 400/500 = Error)
  - Request payload
  - Response body

### 4. Kiểm Tra Python Service
- Đảm bảo Python AI service đang chạy:
  ```powershell
  cd recommend_ai
  python main.py
  # hoặc
  uvicorn main:app --host 0.0.0.0 --port 8000 --reload
  ```
- Test endpoint:
  ```powershell
  Invoke-WebRequest -Uri "http://localhost:8000/" -UseBasicParsing
  ```

### 5. Các Lỗi Thường Gặp

#### Lỗi: "Anti-forgery token not found"
- **Nguyên nhân:** Token không được tìm thấy trong form
- **Giải pháp:** Kiểm tra form `#embeddingForm` có tồn tại không

#### Lỗi: "HTTP 400 Bad Request"
- **Nguyên nhân:** Anti-forgery token không hợp lệ
- **Giải pháp:** Refresh trang và thử lại

#### Lỗi: "HTTP 500 Internal Server Error"
- **Nguyên nhân:** Lỗi server-side
- **Giải pháp:** Kiểm tra logs trong .NET application

#### Lỗi: "Failed to fetch" hoặc "Network error"
- **Nguyên nhân:** Python service không chạy hoặc không thể kết nối
- **Giải pháp:** 
  - Kiểm tra Python service đang chạy
  - Kiểm tra URL trong `appsettings.json`: `AiSettings:ServiceUrl`

#### Lỗi: "Connection refused" hoặc "ECONNREFUSED"
- **Nguyên nhân:** Python service không lắng nghe trên port 8000
- **Giải pháp:** 
  - Kiểm tra Python service đang chạy
  - Kiểm tra firewall
  - Thử truy cập: http://localhost:8000/docs

### 6. Test Thủ Công

#### Test Python Endpoint:
```powershell
$body = @{
    meal_id = 1
    name = "Test Meal"
    ingredients = '["test"]'
    description = "Test"
    calories = 100
    protein = 20.0
    carbs = 10.0
    fat = 5.0
} | ConvertTo-Json

Invoke-WebRequest -Uri "http://localhost:8000/api/generate-meal-embedding" `
    -Method POST `
    -ContentType "application/json" `
    -Body $body
```

#### Test .NET Controller:
- Mở browser console
- Chạy:
```javascript
const formData = new FormData();
const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
formData.append('__RequestVerificationToken', token);

fetch('/Admin/GenerateAllMealEmbeddings', {
    method: 'POST',
    body: formData
})
.then(r => r.json())
.then(console.log)
.catch(console.error);
```

## Checklist

- [ ] Browser console không có lỗi JavaScript
- [ ] Button được tìm thấy (log: "✓ Generate button found")
- [ ] Anti-forgery token được tìm thấy (log: "✓ Anti-forgery token found")
- [ ] Python service đang chạy trên port 8000
- [ ] Có thể truy cập http://localhost:8000/docs
- [ ] Network request được gửi đi (kiểm tra Network tab)
- [ ] Response status code là 200 (không phải 400/500)
