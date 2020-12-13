from urllib import request
import json
import timeit
import pprint

URL = "http://localhost:5000"

def describe():
    resp = request.urlopen(URL + "/")
    return json.loads(resp.read())


def run_raw(c: bytes) -> bytes:
    req = request.Request(URL + "/run", data = c)
    resp = request.urlopen(req)
    return resp.read()

def run(c: list):
    body = json.dumps(c).encode()
    ret = run_raw(body)
    return json.loads(ret)

def test_run():
    run(["DemoDispatchClass", "TargetMethod",  1, "12", ["nested"], [2,3], { "Whoa" : ["deep value 1", "deep2"]}])

def benchmark():
    t = timeit.timeit(test_run, number = 1000)
    print("time ", t, "sec per 1000 reqs (expected ~ 2.4sec total, so 2.4ms per call")

api = describe()
pprint.pprint(api)
benchmark()

