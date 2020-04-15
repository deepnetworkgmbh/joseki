<template>
  <div>
    <Spinner v-if="!loaded" :loadFailed="loadFailed" @reload="loadData" />
    <div v-show="loaded" class="flex flex-col justify-around border rounded-lg mb-3 shadow">
      <div v-if="data.scanResult === 'Failed'" class="p-2">
        <h3>Image vulnerabilities scan failed</h3>
        <span>{{ data.description }}</span>
      </div>

      <div v-if="data.scanResult === 'NotFound'" class="p-2">
        <h3>No Image vulnerabilities scan data</h3>
        <span>Image scanner might not support image OS or container registry</span>
        <span>{{ data.description }}</span>
      </div>

      <div v-if="data.scanResult === 'Succeeded'" class="p-2">
        <ul v-for="(target,i) in data.targets" :key="target.target">
          <li>
            <input class="expand" type="checkbox" :id="`target${i}`" checked />
            <label :for="`target${i}`" class="target">Target: {{ target.target }}</label>
            <ul v-for="(vul, g) in target.vulnerabilities" :key="`${vul.Severity}${i}`">
              <li>
                <input class="expand" type="checkbox" :id="`t${i}g${g}`" checked />
                <label :for="`t${i}g${g}`" class="text-base">
                  {{ vul.Count }} issues with
                  <strong :class="`severity-${vul.Severity}`">{{ vul.Severity }}</strong> severity
                </label>
                <ul v-for="(cve, c) in vul.CVEs" :key="`${vul.Severity}${i}-${c}`" style="margin-bottom:5px;">
                  <li>
                    <input class="expand" type="checkbox" :id="`t${i}g${g}${c}`" checked />
                    <label :for="`t${i}g${g}${c}`" style="font-size:13px;">
                      <strong class="ml-1">{{ cve.vulnerabilityID }}</strong> in
                      <strong>{{ cve.pkgName }}</strong>
                      (version <strong>{{ cve.installedVersion }}</strong>)
                    </label>
                    <ul>
                      <li>
                          <p v-if="cve.title" class="cve-title">{{ cve.Title }}</p>
                          <p class="cve-desc">                            
                            <strong class="remediation-title">Description: </strong>
                            {{ cve.description }}
                          </p>
                          <p v-if="cve.remediation" style="font-size:13px;">
                            <strong class="remediation-title">Remediation: </strong>
                            {{ cve.remediation }}
                          </p>                          
                          <p v-if="cve.dependenciesWithCVE.length>0">
                            <strong class="dependencies-title">Dependencies: </strong>
                            <span class="cve-dependencies">{{ cve.dependenciesWithCVE.join(', ') }}</span>
                          </p>
                          <div v-if="cve.references.length>0" style="margin-top:0px;">
                            <input class="expand" type="checkbox" :id="`t${i}g${g}${c}ref`" />
                            <label :for="`t${i}g${g}${c}ref`" class="references-title">References:</label>
                            <ul>
                              <li v-for="(ref, ri) in cve.references" :key="`ref${ri}`" style="font-size:12px;">
                                &bull;&nbsp;<a :href="ref" class='external-link' target="_blank">{{ ref }}</a>
                              </li>
                            </ul>
                          </div>                        
                      </li>
                    </ul>
                  </li>
                </ul>
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