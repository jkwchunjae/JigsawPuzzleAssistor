import cv2
import numpy as np

# 한 점이 다른 선분에 얼마나 떨어져있는지 확인
def distance(x1, y1, x2, y2, x3, y3):
    # x1, y1 은 점
    # x2, y2 는 선분의 시작점
    # x3, y3 는 선분의 끝점
    px = x3 - x2
    py = y3 - y2
    something = px * px + py * py
    u = ((x1 - x2) * px + (y1 - y2) * py) / float(something)
    if u > 1:
        u = 1
    elif u < 0:
        u = 0
    x = x2 + u * px
    y = y2 + u * py
    dx = x - x1
    dy = y - y1
    dist = np.sqrt(dx * dx + dy * dy)
    return dist

# 이미지를 불러오고 그레이스케일로 변환
image = cv2.imread('20230804_225655_3.jpg')
# image = cv2.imread('20230806_161402_3.jpg')
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
corners = cv2.goodFeaturesToTrack(outline, maxCorners=4, qualityLevel=0.01, minDistance=100)
corners = np.int0(corners)
for corner in corners:
    x, y = corner.ravel()
    # cv2.circle(output, (x, y), 5, (255, 0, 255), -1)

x0, y0 = corners[0].ravel()
x1, y1 = corners[1].ravel()
x2, y2 = corners[2].ravel()
x3, y3 = corners[3].ravel()

len1 = np.sqrt((x0 - x1) ** 2 + (y0 - y1) ** 2)
len2 = np.sqrt((x0 - x2) ** 2 + (y0 - y2) ** 2)
len3 = np.sqrt((x0 - x3) ** 2 + (y0 - y3) ** 2)

# 0번 코너와 3번 코너는 대각선으로 세팅
if len1 > len2 and len1 > len3:
    x1, x3 = x3, x1
    y1, y3 = y3, y1
elif len2 > len1 and len2 > len3:
    x2, x3 = x3, x2
    y2, y3 = y3, y2

e01 = [] # 0번 코너와 1번 코너 사이의 선분 위의 점들
e02 = [] # 0번 코너와 2번 코너 사이의 선분 위의 점들
e13 = [] # 1번 코너와 3번 코너 사이의 선분 위의 점들
e23 = [] # 2번 코너와 3번 코너 사이의 선분 위의 점들
e99 = []

prevIndex = 0
for point in puzzle_contour:
    x, y = point.ravel()
    d01 = distance(x, y, x0, y0, x1, y1)
    d02 = distance(x, y, x0, y0, x2, y2)
    d13 = distance(x, y, x1, y1, x3, y3)
    d23 = distance(x, y, x2, y2, x3, y3)

    # d01, d02, d13, d23 중 가장 작은 값의 인덱스를 구함
    min_index = np.argmin([d01, d02, d13, d23])
    min_distance = np.min([d01, d02, d13, d23])

    if min_distance < 10:
        [e01, e02, e13, e23][min_index].append((x, y))
        prevIndex = min_index
    else:
        [e01, e02, e13, e23][prevIndex].append((x, y))
        e99.append((x, y))


for p in e01:
    cv2.circle(output, p, 3, (0, 0, 255), -1)
for p in e02:
    cv2.circle(output, p, 3, (0, 255, 0), -1)
for p in e13:
    cv2.circle(output, p, 3, (255, 0, 255), -1)
for p in e23:
    cv2.circle(output, p, 3, (255, 255, 0), -1)
# for p in e99:
#     cv2.circle(output, p, 3, (255, 255, 255), -1)

# 결과 이미지 출력
cv2.imshow('Puzzle Area', output)
cv2.waitKey(0)
cv2.destroyAllWindows()
