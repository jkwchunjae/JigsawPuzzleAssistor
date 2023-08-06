import cv2
import numpy as np

# 이미지를 불러오고 그레이스케일로 변환
image = cv2.imread('20230804_225655_3.jpg')
# image = cv2.imread('20230806_161402.jpg')
# image = cv2.resize(image, dsize=(0, 0), fx=0.2, fy=0.2, interpolation=cv2.INTER_AREA)
gray = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)
gaussian = cv2.GaussianBlur(gray, (5, 5), 0)
_, black_white = cv2.threshold(gaussian, 127, 255, cv2.THRESH_BINARY)

# 경계선 검출 (Canny edge detection 예제)
edges = cv2.Canny(black_white, 50, 255)

# 경계선 검출 결과에서 퍼즐 조각의 외곽을 감싸는 경계 상자를 찾음
contours, _ = cv2.findContours(edges, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
puzzle_contour = max(contours, key=cv2.contourArea)
outline = np.zeros(image.shape, np.uint8)
output = np.zeros(image.shape, np.uint8)
cv2.drawContours(outline, [puzzle_contour], -1, (0, 255, 0), 1);
cv2.drawContours(output, [puzzle_contour], -1, (0, 255, 0), 1);

# 퍼즐 조각의 외곽을 감싸는 경계 상자를 이용하여 코너 추출
outline = cv2.cvtColor(outline, cv2.COLOR_BGR2GRAY)
corners1 = cv2.goodFeaturesToTrack(outline, maxCorners=4, qualityLevel=0.01, minDistance=100)
if corners1 is not None:
    corners1 = np.int0(corners1)
    for corner in corners1:
        x, y = corner.ravel()
        cv2.circle(output, (x, y), 5, (255, 0, 255), -1)

# 결과 이미지 출력
cv2.imshow('Puzzle Area', output)
cv2.waitKey(0)
cv2.destroyAllWindows()