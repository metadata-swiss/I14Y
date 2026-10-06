// plugins/swiper.client.ts
import { register } from 'swiper/element/bundle'
// Optional CSS (see next point)
import 'swiper/css'
// or import the full bundle styles:
// import 'swiper/css/bundle'

export default defineNuxtPlugin(() => {
  register()
})
