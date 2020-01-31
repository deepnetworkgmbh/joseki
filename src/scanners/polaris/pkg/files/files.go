package files

import (
	"bytes"
	"encoding/json"
	"io"
	"os"
)

func Save(path string, v interface{}) error {
	f, err := os.Create(path)
	if err != nil {
		return err
	}
	defer f.Close()

	b, err := json.MarshalIndent(v, "", "\t")

	if err != nil {
		return err
	}

	_, err = io.Copy(f, bytes.NewReader(b))
	return err
}


func Delete(path string) error {
	return os.Remove(path)
}