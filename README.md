# Spider-Man VR Haptics Experience

## Overview

XR Bootcampにおいて制作したVRコンテンツです。

Meta Questのハンドトラッキングと
TapeTicsの振動触覚デバイスを用いて、
スパイダーマンのように糸を発射できる体験を実現しました。
## Demo


GameScene

https://github.com/user-attachments/assets/ec3119f6-fc98-403c-bdb8-c24f5ead7ac4


RealScene

https://github.com/user-attachments/assets/54e88cdb-0cc3-46e2-8976-550c11e247ce

Pose Recognition and Wrist Device Feedback

The system uses an Intel RealSense camera to capture the user's hand and body movements and recognize predefined poses. When a specific pose is detected, a trigger signal is sent to a wrist-worn device. The device then provides feedback through LED lighting and vibration, allowing the user to immediately perceive that the pose has been successfully recognized. By combining visual and haptic feedback, the system creates a more immersive and interactive user experience.


## Features

- Hand Tracking
- Pose Recognition
- Web Shooting
- Haptic Feedback

## Technologies

- Unity
- C#
- Meta Quest
- Meta XR SDK
- TapeTics

## My Contributions

- ハンドトラッキング実装
- ポーズ認識実装
- 糸発射処理実装
- 振動フィードバック連携

## Project Background

This project was developed during an XR Bootcamp in a two-person team.

Our goal was to recreate the feeling of becoming Spider-Man by combining hand tracking and haptic feedback technologies.

By performing the iconic Spider-Man hand gesture, users can shoot webs and feel vibrations on their wrist through a wearable haptic device.

