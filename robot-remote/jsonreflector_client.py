from typing import List
from urllib import request
import json
import timeit
import pprint
from dataclasses import dataclass


URL = "http://localhost:5000"


def describe():
    resp = request.urlopen(URL + "/")
    return json.loads(resp.read())


def run_raw(c: bytes, session_id: str) -> bytes:
    req = request.Request(URL + "/run", data=c)
    req.add_header("x-remote-session-id", session_id)

    resp = request.urlopen(req)
    return resp.read()


def run(c: list, session_id: str) -> dict:
    body = json.dumps(c).encode()
    ret = run_raw(body, session_id)
    return json.loads(ret)


def test_run():
    run(
        [
            "App.DemoDispatchClass",
            "TargetMethod",
            1,
            "12",
            ["nested"],
            [2, 3],
            {"Whoa": ["deep value 1", "deep2"]},
        ],
        "foo",
    )


def benchmark():
    t = timeit.timeit(test_run, number=1000)
    print("time ", t, "sec per 1000 reqs (expected ~ 2.4sec total, so 2.4ms per call")


@dataclass
class Keyword:
    klass: str
    method: str


def get_keyword_names() -> List[Keyword]:
    desc = describe()
    all = []
    print(desc)
    for c in desc:
        name, _, methods, *o = c
        for m in methods:
            all.append(Keyword(name, m[0]))

    return all


def run_keyword(kw: Keyword, args: list):
    call = [kw.klass, kw.method] + list(args)
    ret = run(call, "myses")
    if ret["Exc"]:
        # xxx exc class
        raise ret["Exc"]

    return ret["R"]
