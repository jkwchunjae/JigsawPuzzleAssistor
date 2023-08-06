import cv2

# 이미지를 불러오고 그레이스케일로 변환
image = cv2.imread('20230804_225655_3.jpg')
gray = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)
gaussian = cv2.GaussianBlur(gray, (5, 5), 0)
_, black_white = cv2.threshold(gaussian, 127, 255, cv2.THRESH_BINARY)

# 경계선 검출 (Canny edge detection 예제)
edges = cv2.Canny(black_white, 50, 255)

cv2.imshow('gray', gray);
cv2.imshow('gaussian', gaussian);
cv2.imshow('black_white', black_white);

# 경계선 검출 결과에서 퍼즐 조각의 외곽을 감싸는 경계 상자를 찾음
contours, _ = cv2.findContours(edges, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
puzzle_contour = max(contours, key=cv2.contourArea)
cv2.drawContours(image, [puzzle_contour], -1, (0, 255, 0), 2);

# 퍼즐 영역 추출
# x, y, w, h = cv2.boundingRect(puzzle_contour)
# puzzle_area = image[y:y+h, x:x+w]

# 결과 이미지 출력
cv2.imshow('Puzzle Area', image)
cv2.waitKey(0)
cv2.destroyAllWindows()