<template>
  <v-row justify="center">
    <v-col cols="12" md="6">
      <v-card class="pa-6 text-center text-surface-variant relative-card">
        <h2 class="text-h5 font-weight-bold mb-2">Trạm Chấm công Điện tử</h2>
        <p class="text-subtitle-2 text-disabled mb-6">Phân hệ Chấm công Thời gian thực - Attendance Service</p>

        <div class="time-display my-6">
          <div class="text-h2 font-weight-black text-white tracking-widest">{{ currentTime }}</div>
          <div class="text-subtitle-1 text-primary font-weight-bold mt-2">{{ currentDate }}</div>
        </div>

        <v-sheet class="mx-auto rounded-xl face-mockup mb-6 d-flex flex-column align-center justify-center relative-card" max-width="340" height="220" color="black">
          <v-icon size="64" color="rgba(255,255,255,0.15)">mdi-face-recognition</v-icon>
          <div class="face-scan-line"></div>
          <div class="text-caption text-success mt-2 font-weight-bold absolute-bottom">
            <v-icon size="14" color="success" class="mr-1">mdi-map-marker-radius</v-icon>
            Định vị GPS: Trụ sở chính văn phòng (Hợp lệ)
          </div>
        </v-sheet>

        <v-row>
          <v-col cols="6">
            <v-btn block size="large" color="success" class="gradient-btn-green py-4" height="54" prepend-icon="mdi-login" @click="handleCheckIn">
              Check-In Ca Sáng
            </v-btn>
          </v-col>
          <v-col cols="6">
            <v-btn block size="large" color="error" variant="outlined" height="54" prepend-icon="mdi-logout" @click="handleCheckOut">
              Check-Out Ca Chiều
            </v-btn>
          </v-col>
        </v-row>
      </v-card>
    </v-col>
  </v-row>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue'

const currentTime = ref('08:00:00')
const currentDate = ref('')

const updateTime = () => {
  const now = new Date()
  currentTime.value = now.toTimeString().split(' ')[0]
  currentDate.value = now.toLocaleDateString('vi-VN', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' })
}

let timer
onMounted(() => {
  updateTime()
  timer = setInterval(updateTime, 1000)
})
onUnmounted(() => clearInterval(timer))

const handleCheckIn = () => alert('Đã Check-In thành công qua FaceID & GPS!')
const handleCheckOut = () => alert('Đã Check-Out thành công!')
</script>

<style scoped>
.time-display {
  font-family: 'Courier New', Courier, monospace;
}
.face-mockup {
  border: 2px solid rgba(99, 102, 241, 0.4);
  position: relative;
  overflow: hidden;
}
.face-scan-line {
  position: absolute;
  width: 100%;
  height: 2px;
  background: linear-gradient(to right, transparent, #00f2fe, transparent);
  animation: scan 2.5s infinite linear;
}
@keyframes scan {
  0% { top: 0%; }
  50% { top: 100%; }
  100% { top: 0%; }
}
.absolute-bottom {
  position: absolute;
  bottom: 12px;
}
</style>