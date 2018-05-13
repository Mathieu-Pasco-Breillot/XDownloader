<template>
  <div class="home">
    <img src="../assets/logo.png" width="200px">

    <div class="grid">
      <div class="protector">
        <label for="protector">Get link from protector&nbsp;:</label>
        <input type="text" placeholder="protector link">
      </div>
      
      <div class="source">
        <label for="source">Get link from source&nbsp;:</label>
        <input type="text" placeholder="protector link">
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';
// import HelloWorld from "@/components/HelloWorld.vue"; // @ is an alias to /src

@Component({
  components: {},
})
export default class Home extends Vue {}
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>
.grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  grid-gap: 2em;
}

</style>