//
// StartTimerIntent.m
//
// This file was automatically generated and should not be edited.
//

#import "StartTimerIntent.h"

#if __has_include(<Intents/Intents.h>) && (!TARGET_OS_OSX || TARGET_OS_IOSMAC) && !TARGET_OS_TV

@implementation StartTimerIntent

@dynamic workspace, entryDescription, billable, projectId, taskId, tags;

@end

@interface StartTimerIntentResponse ()

@property (readwrite, NS_NONATOMIC_IOSONLY) StartTimerIntentResponseCode code;

@end

@implementation StartTimerIntentResponse

@synthesize code = _code;

- (instancetype)initWithCode:(StartTimerIntentResponseCode)code userActivity:(nullable NSUserActivity *)userActivity {
    self = [super init];
    if (self) {
        _code = code;
        self.userActivity = userActivity;
    }
    return self;
}

@end

#endif
