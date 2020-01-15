<template>
  <div>
    <Spinner v-if="!loaded" class="centered" />
    <div v-show="loaded" class="segment shadow">
      <div style="border-bottom: solid 1px #ddd;background-color: #eee;">
        <h3 :class="`scanStatus image-result-${data.scanResult}`">{{ data.scanResult }}</h3>
        <h1 class="image-name" style="margin-left: 15px;">{{ imageid }}</h1>
      </div>

      <div v-if="data.scanResult === 'Failed'" class="scan-info">
        <h3>Image vulnerabilities scan failed</h3>
        <span>{{ data.description }}</span>
      </div>

      <div v-if="data.scanResult === 'NotFound'" class="scan-info">
        <h3>No Image vulnerabilities scan data</h3>
        <span>Image scanner might not support image OS or container registry</span>
        <span>{{ data.description }}</span>
      </div>

      <div v-if="data.scanResult === 'Succeeded'" class="scan-info">
        <ul v-for="(target,i) in data.targets" :key="target.target">
          <li>
            <input type="checkbox" :id="`target${i}`" checked />
            <label :for="`target${i}`">Target: {{ target.target }}</label>
            <ul v-for="(vul, g) in target.vulnerabilities" :key="vul.VulnerabilityID">
              <li>
                <input type="checkbox" :id="`t${i}g${g}`" checked />
                <label :for="`t${i}g${g}`">
                  {{ vul.Count }} issues with
                  <strong
                    :class="`severity-${vul.Severity}`"
                  >{{ vul.Severity }}</strong>
                  severity
                </label>
              </li>
            </ul>
          </li>
        </ul>
      </div>
    </div>
  </div>
</template>
<script lang="ts" src="./ImageDetail.ts"></script>
<style lang="scss" src="./ImageDetail.scss"></style>