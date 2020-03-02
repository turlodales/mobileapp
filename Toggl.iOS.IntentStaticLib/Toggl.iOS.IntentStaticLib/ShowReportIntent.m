//
// ShowReportIntent.m
//
// This file was automatically generated and should not be edited.
//

#import "ShowReportIntent.h"

#if __has_include(<Intents/Intents.h>) && (!TARGET_OS_OSX || TARGET_OS_IOSMAC) && !TARGET_OS_TV

@implementation ShowReportIntent

@end

@interface ShowReportIntentResponse ()

@property (readwrite, NS_NONATOMIC_IOSONLY) ShowReportIntentResponseCode code;

@end

@implementation ShowReportIntentResponse

@synthesize code = _code;

- (instancetype)initWithCode:(ShowReportIntentResponseCode)code userActivity:(nullable NSUserActivity *)userActivity {
    self = [super init];
    if (self) {
        _code = code;
        self.userActivity = userActivity;
    }
    return self;
}

@end

#endif
