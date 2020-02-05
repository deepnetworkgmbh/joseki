package azureblob

import (
	"encoding/hex"
	"fmt"
	"math/rand"
	"time"
)

var src = rand.New(rand.NewSource(time.Now().UnixNano()))

func GenerateAuditFolderName(timePart time.Time) string{
	year := fmt.Sprintf("%04d", timePart.Year())
	month := fmt.Sprintf("%02d", timePart.Month())
	day := fmt.Sprintf("%02d", timePart.Day())

	hour := fmt.Sprintf("%02d", timePart.Hour())
	minute := fmt.Sprintf("%02d", timePart.Minute())
	second := fmt.Sprintf("%02d", timePart.Second())

	folderName := fmt.Sprintf("%v%v%v-%v%v%v-%v", year, month, day, hour, minute, second, generateHexRandom(6))

	return folderName
}

// RandStringBytesMaskImprSrc returns a random hexadecimal string of length n.
func generateHexRandom(n int) string {
	b := make([]byte, n/2)

	if _, err := src.Read(b); err != nil {
		panic(err)
	}

	return hex.EncodeToString(b)[:n]
}
