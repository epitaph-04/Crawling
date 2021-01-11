package utils

import "sync"

// ConcurrentSlice struct hold items to be append concurrently
type ConcurrentSlice struct {
	sync.RWMutex
	items []interface{}
}

// NewConcurrentSlice function to construct ConcurrentSlice
func NewConcurrentSlice() *ConcurrentSlice {
	cs := &ConcurrentSlice{
		items: make([]interface{}, 0),
	}
	return cs
}

// Append function for concurrent append
func (cs *ConcurrentSlice) Append(item interface{}) {
	cs.Lock()
	defer cs.Unlock()

	cs.items = append(cs.items, item)
}

// Iter func support
func (cs *ConcurrentSlice) Iter() <-chan interface{} {
	c := make(chan interface{})

	f := func() {
		cs.Lock()
		defer cs.Unlock()
		for _, value := range cs.items {
			c <- value
		}
		close(c)
	}
	go f()

	return c
}
